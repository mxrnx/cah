using Microsoft.Extensions.Caching.Memory;

namespace Server.Services;

using Microsoft.AspNetCore.Http;

public interface ISessionService
{
    /// <summary>
    /// Initializes a new session, setting the current player's id to the given Guid.
    /// </summary>
    /// <param name="playerId">Guid for this session's player.</param>
    void CreateSession(Guid playerId);

    /// <summary>
    /// Get the current player's id, if any.
    /// </summary>
    /// <returns>Guid of the current player if one exists, otherwise null.</returns>
    Guid? GetCurrentPlayerId();

    void RemoveSession();
}

public class SessionService(IHttpContextAccessor httpContextAccessor, IMemoryCache cache) : ISessionService
{
    public void CreateSession(Guid playerId)
    {
        cache.Set(ConnectionKey(), playerId);
    }

    public Guid? GetCurrentPlayerId()
    {
        if (cache.TryGetValue(ConnectionKey(), out Guid playerIdBytes))
            return playerIdBytes;
        return null;
    }

    public void RemoveSession() =>
        cache.Remove(ConnectionKey());

    /// <summary>
    /// Gets the current HttpContext, throwing an exception if not successful.
    /// </summary>
    /// <returns>Current HttpContext.</returns>
    /// <exception cref="InvalidOperationException">Thrown when accessing the context fails.</exception>
    private HttpContext GetContext() =>
        httpContextAccessor.HttpContext ?? throw new InvalidOperationException("Could not access HttpContext");

    /// <summary>
    /// Returns the memory cache key for this specific connection.
    /// </summary>
    /// <returns></returns>
    private string ConnectionKey() =>
        GetContext().Connection.Id;
}
