using Microsoft.Extensions.Caching.Memory;
using Server.Enums;

namespace Server.Services;

public class GameService
{
    private readonly MemoryCache _cache;

    private const string KEY_CZAR = "czar";
    private const string KEY_WINS = "wins";
    
    public GameService()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    public void SetNecessaryWins(int necessaryWins) =>
        _cache.Set(KEY_WINS, necessaryWins);

    public int GetNecessaryWins() =>
        _cache.TryGetValue<int>(KEY_WINS, out var necessaryWins)
            ? necessaryWins
            : 0;
    
    public void SetGameState(EGameState gameState) =>
        _cache.Set(KEY_WINS, gameState);

    public EGameState GetGameState() =>
        _cache.TryGetValue<EGameState>(KEY_WINS, out var gameState)
            ? gameState
            : EGameState.NotStarted;
    
    public void SetCzar(Guid id) =>
        _cache.Set(KEY_CZAR, id);

    public Guid GetCzar() =>
        _cache.TryGetValue<Guid>(KEY_CZAR, out var czar)
            ? czar
            : Guid.Empty;
}
