using Microsoft.AspNetCore.Mvc;
using Server.Enums;
using Server.Models.Dtos;
using Server.Services;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class GameController : ControllerBase
{
    private readonly CahContext _context;
    private readonly GameService _gameService;
    private readonly SessionService _sessionService;

    public GameController(CahContext context, GameService gameService, SessionService sessionService)
    {
        _context = context;
        _gameService = gameService;
        _sessionService = sessionService;
    }
    
    [HttpPost]
    public async Task<ActionResult<HandDto>> Post([FromBody] int necessaryWins)
    {
        if (_gameService.GetGameState() != EGameState.NotStarted)
            return BadRequest("Game has already started.");
        
        var currentPlayerId = _sessionService.GetCurrentPlayerId();
        if (currentPlayerId is null)
            return BadRequest("Player not logged in.");
        
        if (necessaryWins is < 1 or > 20)
            return BadRequest("Number of necessary wins must be greater than 1 and less than 20.");

        if (currentPlayerId != _gameService.GetCzar())
            return Unauthorized("Game has already started.");
        
        _gameService.SetNecessaryWins(necessaryWins);

        foreach (var player in _context.Players)
        {
            // player. // TODO: deal cards
        }
        
        return Ok(new HandDto()); // TODO: retrieve current player's hand
    }
}
