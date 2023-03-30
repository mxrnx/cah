namespace Server.Services;

using Microsoft.AspNetCore.Http;

public class SessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    private const string PLAYER_ID = "_PlayerId";

    public SessionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    /// <summary>
    /// Initializes a new session, setting the current player's id to the given Guid.
    /// </summary>
    /// <param name="playerID">Guid for this session's player.</param>
    public void CreateSession(Guid playerID)
    {
        GetContext().Session.Set(PLAYER_ID, playerID.ToByteArray());
    }

    /// <summary>
    /// Get the current player's id, if any.
    /// </summary>
    /// <returns>Guid of the current player if one exists, otherwise null.</returns>
    public Guid? GetCurrentPlayerId()
    {
        if (GetContext().Session.TryGetValue(PLAYER_ID, out var playerIdBytes))
            return new Guid(playerIdBytes);
        return null;

    }

    /// <summary>
    /// Gets the current HttpContext, throwing an exception if not successful.
    /// </summary>
    /// <returns>Current HttpContext.</returns>
    /// <exception cref="InvalidOperationException">Thrown when accessing the context fails.</exception>
    private HttpContext GetContext() =>
        _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("Could not access HttpContext");
}
