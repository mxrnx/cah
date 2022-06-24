using Microsoft.AspNetCore.Mvc;

using Server.Models;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class PlayerController : ControllerBase
{
    private readonly ILogger<PlayerController> _logger;

    public PlayerController(ILogger<PlayerController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public Player Post([FromBody] String pc)
    {
        // TODO: check if player is not yet logged in/has no session
        // TODO: verify name validity
        return new Player(Guid.NewGuid(), pc);
    }

    [HttpGet]
    public IEnumerable<Player> Get()
    {
        return Array.Empty<Player>();
    }
}
