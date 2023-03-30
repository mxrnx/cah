using Microsoft.AspNetCore.Mvc;
using Server.Services;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class CzarController : ControllerBase
{
    private readonly GameService _gameService;

    public CzarController(GameService gameService)
    {
        _gameService = gameService;
    }

    [HttpGet]
    public ActionResult<Guid> Get() =>
        _gameService.GetCzar();

}
