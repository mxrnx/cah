using Microsoft.Extensions.Caching.Memory;

namespace Server.Services;

using Microsoft.AspNetCore.Http;

public class SessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemoryCache _cache;

    public SessionService(IHttpContextAccessor httpContextAccessor, IMemoryCache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _cache = cache;
    }
    
    /// <summary>
    /// Initializes a new session, setting the current player's id to the given Guid.
    /// </summary>
    /// <param name="playerID">Guid for this session's player.</param>
    public void CreateSession(Guid playerID)
    {
        _cache.Set(ConnectionKey(), playerID);
    }

    /// <summary>
    /// Get the current player's id, if any.
    /// </summary>
    /// <returns>Guid of the current player if one exists, otherwise null.</returns>
    public Guid? GetCurrentPlayerId()
    {
        if (_cache.TryGetValue(ConnectionKey(), out Guid playerIdBytes))
            return playerIdBytes;
        return null;

    }

    /// <summary>
    /// Gets the current HttpContext, throwing an exception if not successful.
    /// </summary>
    /// <returns>Current HttpContext.</returns>
    /// <exception cref="InvalidOperationException">Thrown when accessing the context fails.</exception>
    private HttpContext GetContext() =>
        _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("Could not access HttpContext");

    /// <summary>
    /// Returns the memory cache key for this specific connection.
    /// </summary>
    /// <returns></returns>
    private string ConnectionKey() =>
        GetContext().Connection.Id;
}
