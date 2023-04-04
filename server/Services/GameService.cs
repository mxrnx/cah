using Microsoft.Extensions.Caching.Memory;
using Server.Enums;
using Server.Models;
using Server.Models.Entities;

namespace Server.Services;

/// <summary>
/// Singleton service containing global state about the game.
/// </summary>
public class GameService
{
    private readonly MemoryCache _cache;
    private Deck[] _decksInPlay = Array.Empty<Deck>();
    private DrawPile<AnswerCard>? _answerCardsDrawPile;
    private DrawPile<PromptCard>? _promptCardsDrawPile;

    private const string KEY_CZAR = "czar";
    private const string KEY_WINS = "wins";
    
    public GameService()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
    }
    
    /// <summary>
    /// Initializes a new game.
    /// </summary>
    /// <param name="necessaryWins">The amount of rounds won a player needs to win the game.</param>
    /// <param name="decks">The decks that will be used in the game.</param>
    public void SetupGame(int necessaryWins, IEnumerable<Deck> decks)
    {
        SetGameState(EGameState.Started);
        _cache.Set(KEY_WINS, necessaryWins);
        _decksInPlay = decks.ToArray();
        _answerCardsDrawPile = new DrawPile<AnswerCard>(_decksInPlay.SelectMany(x => x.AnswerCards));
        _promptCardsDrawPile = new DrawPile<PromptCard>(_decksInPlay.SelectMany(x => x.PromptCards));
    }

    public IEnumerable<AnswerCard> DrawAnswerCards(int count) =>
        _answerCardsDrawPile?.DrawCards(count) ?? Array.Empty<AnswerCard>();

    public PromptCard DrawPromptCard() =>
        _promptCardsDrawPile?.DrawCard()!; // TODO: fix nullability issue

    public int GetNecessaryWins() =>
        _cache.TryGetValue<int>(KEY_WINS, out var necessaryWins)
            ? necessaryWins
            : throw new InvalidOperationException("Amount of necessary wins not yet set.");

    private void SetGameState(EGameState gameState) =>
        _cache.Set(KEY_WINS, gameState);

    public EGameState GetGameState() =>
        _cache.TryGetValue<EGameState>(KEY_WINS, out var gameState)
            ? gameState
            : EGameState.NotStarted;
    
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
}
