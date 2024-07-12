using Microsoft.Extensions.Caching.Memory;
using Server.Enums;
using Server.Models;
using Server.Models.Entities;

namespace Server.Services;

public interface IGameService
{
    /// <summary>
    /// Initializes a new game.
    /// </summary>
    /// <param name="necessaryWins">The amount of rounds won a player needs to win the game.</param>
    /// <param name="decks">The decks that will be used in the game.</param>
    void SetupGame(int necessaryWins, IEnumerable<Deck> decks);
    
    IEnumerable<AnswerCard> DrawAnswerCards(int count);
    EGamePhase GetGamePhase();
    int GetNecessaryWins();
    PromptCard GetPromptCard();

    /// <summary>
    /// Sets the Guid of the current Card Czar.
    /// </summary>
    void SetCzar(Guid id);

    /// <summary>
    /// Returns the Guid of the current Card Czar.
    /// </summary>
    Guid GetCzar();
}

/// <summary>
/// Singleton service containing global state about the game.
/// </summary>
public sealed class GameService : IGameService, IDisposable
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private Deck[] _decksInPlay = [];
    private DrawPile<AnswerCard> _answerCardsDrawPile = new(Array.Empty<AnswerCard>());
    private DrawPile<PromptCard> _promptCardsDrawPile = new(Array.Empty<PromptCard>());

    private const string KeyCzar = "czar";
    private const string KeyPhase = "phase";
    private const string KeyWins = "wins";
    private const string KeyPrompt = "prompt";
    private bool _disposed;

    public void SetupGame(int necessaryWins, IEnumerable<Deck> decks)
    {
        SetNecessaryWins(necessaryWins);
        
        _decksInPlay = decks.ToArray();
        _answerCardsDrawPile = new DrawPile<AnswerCard>(_decksInPlay.SelectMany(x => x.AnswerCards));
        _promptCardsDrawPile = new DrawPile<PromptCard>(_decksInPlay.SelectMany(x => x.PromptCards));
        
        DrawPromptCard();
        SetGameState(EGamePhase.PickingAnswers);
    }
    
    public IEnumerable<AnswerCard> DrawAnswerCards(int count) =>
        _answerCardsDrawPile.DrawCards(count);

    public EGamePhase GetGamePhase() =>
        _cache.TryGetValue<EGamePhase>(KeyPhase, out var gameState)
            ? gameState
            : EGamePhase.WaitingToStart;
    
    public int GetNecessaryWins() =>
        _cache.TryGetValue<int>(KeyWins, out var necessaryWins)
            ? necessaryWins
            : throw new InvalidOperationException("Amount of necessary wins not yet set.");
    
    public PromptCard GetPromptCard()
    {
        if (!_cache.TryGetValue<PromptCard>(KeyPrompt, out var promptCard) || promptCard is null)
            throw new InvalidOperationException("Amount of necessary wins not yet set.");
        
        return promptCard;
    }

    public void SetCzar(Guid id) =>
        _cache.Set(KeyCzar, id);

    public Guid GetCzar() =>
        _cache.TryGetValue<Guid>(KeyCzar, out var czar)
            ? czar
            : Guid.Empty;
    
    public void Dispose()
    {
        if (_disposed)
            return;

        _cache.Dispose();

        _disposed = true;
    }
    
    private void SetGameState(EGamePhase gamePhase) =>
        _cache.Set(KeyPhase, gamePhase);
    
    private void SetNecessaryWins(int necessaryWins) =>
        _cache.Set(KeyWins, necessaryWins);

    private void SetPromptCard(PromptCard card) =>
        _cache.Set(KeyPrompt, card);
    
    private void DrawPromptCard() =>
        SetPromptCard(_promptCardsDrawPile.DrawCard());
}
