using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Enums;
using Server.Extensions;
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

    private const int CARDS_IN_FULL_HAND = 8;

    public GameController(CahContext context, GameService gameService, SessionService sessionService)
    {
        _context = context;
        _gameService = gameService;
        _sessionService = sessionService;
    }
    
    [HttpPost]
    public async Task<ActionResult> Post([FromBody] int necessaryWins)
    {
        if (_gameService.GetGameState() != EGameState.NotStarted)
            return BadRequest("Game has already started.");
        
        var currentPlayerId = _sessionService.GetCurrentPlayerId();
        if (currentPlayerId is null)
            return Unauthorized("Player not logged in.");
        
        if (currentPlayerId != _gameService.GetCzar())
            return Unauthorized("Player is not the Card Czar.");
        
        if (necessaryWins is < 1 or > 20)
            return BadRequest("Number of necessary wins must be greater than 1 and less than 20.");

        // TODO: make it possible to select decks
        _gameService.SetupGame(necessaryWins, _context.Decks.Include(x => x.AnswerCards).Include(x => x.PromptCards));

        foreach (var player in _context.Players)
            player.CardsInHand.AddMany(_gameService.DrawAnswerCards(CARDS_IN_FULL_HAND));

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<GameDto>> Get()
    {
        var player = await _context.Players.Include(x => x.CardsInHand)
            .SingleOrDefaultAsync(x => x.Id == _sessionService.GetCurrentPlayerId());
        if (player is null)
            return Unauthorized("Player not logged in.");

        return Ok(new GameDto(player.CardsInHand));
    }
}
