using Microsoft.Extensions.Caching.Memory;
using Server.Enums;
using Server.Models;
using Server.Models.Entities;

namespace Server.Services;

/// <summary>
/// Singleton service containing global state about the game.
/// </summary>
public sealed class GameService : IDisposable
{
    private readonly MemoryCache _cache;
    private Deck[] _decksInPlay = Array.Empty<Deck>();
    private DrawPile<AnswerCard> _answerCardsDrawPile;
    private DrawPile<PromptCard> _promptCardsDrawPile;

    private const string KEY_CZAR = "czar";
    private const string KEY_PHASE = "phase";
    private const string KEY_WINS = "wins";
    private const string KEY_PROMPT = "prompt";
    private bool _disposed;

    public GameService()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _answerCardsDrawPile = new DrawPile<AnswerCard>(Array.Empty<AnswerCard>());
        _promptCardsDrawPile = new DrawPile<PromptCard>(Array.Empty<PromptCard>());
    }
    
    /// <summary>
    /// Initializes a new game.
    /// </summary>
    /// <param name="necessaryWins">The amount of rounds won a player needs to win the game.</param>
    /// <param name="decks">The decks that will be used in the game.</param>
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

    public void DrawPromptCard() =>
        SetPromptCard(_promptCardsDrawPile.DrawCard());

    public EGamePhase GetGamePhase() =>
        _cache.TryGetValue<EGamePhase>(KEY_PHASE, out var gameState)
            ? gameState
            : EGamePhase.WaitingToStart;
    
    public int GetNecessaryWins() =>
        _cache.TryGetValue<int>(KEY_WINS, out var necessaryWins)
            ? necessaryWins
            : throw new InvalidOperationException("Amount of necessary wins not yet set.");
    
    public PromptCard GetPromptCard()
    {
        if (!_cache.TryGetValue<PromptCard>(KEY_PROMPT, out var promptCard) || promptCard is null)
            throw new InvalidOperationException("Amount of necessary wins not yet set.");
        
        return promptCard;
    }

    /// <summary>
    /// Sets the Guid of the current Card Czar.
    /// </summary>
    public void SetCzar(Guid id) =>
        _cache.Set(KEY_CZAR, id);

    /// <summary>
    /// Returns the Guid of the current Card Czar.
    /// </summary>
    public Guid GetCzar() =>
        _cache.TryGetValue<Guid>(KEY_CZAR, out var czar)
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
        _cache.Set(KEY_PHASE, gamePhase);
    
    private void SetNecessaryWins(int necessaryWins) =>
        _cache.Set(KEY_WINS, necessaryWins);

    private void SetPromptCard(PromptCard card) =>
        _cache.Set(KEY_PROMPT, card);

}
