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
        if (_gameService.GetGamePhase() != EGamePhase.WaitingToStart)
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

    [HttpPost("Card")]
    public async Task<ActionResult> PostCard([FromBody] Guid cardId)
    {
        if (_gameService.GetGamePhase() != EGamePhase.PickingAnswers)
            return BadRequest("Wrong game phase.");
        
        var currentPlayerId = _sessionService.GetCurrentPlayerId();
        if (currentPlayerId is null)
            return Unauthorized("Player not logged in.");
        
        if (currentPlayerId == _gameService.GetCzar())
            return Unauthorized("Player is the Card Czar.");

        var currentPlayer = await _context.Players.SingleAsync(x => x.Id == currentPlayerId);
        if (currentPlayer.CardsThisRound.Count >= _gameService.GetPromptCard().FieldCount)
            return BadRequest("Player already played maximum amount of cards.");

        var card = await _context.AnswerCards.SingleAsync(x => x.Id == cardId);
        currentPlayer.CardsThisRound.Add(card);
        currentPlayer.CardsInHand.Remove(card);
        await _context.SaveChangesAsync();

        if (_context.Players.Any(p => p.CardsThisRound.Count != _gameService.GetPromptCard().FieldCount))
            return NoContent(); // Round is not finished yet, so we're done here

        // TODO: go to next phase
        
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<GameDto>> Get()
    {
        var player = await _context.Players.Include(x => x.CardsInHand)
            .SingleOrDefaultAsync(x => x.Id == _sessionService.GetCurrentPlayerId());
        if (player is null)
            return Unauthorized("Player not logged in.");

        return Ok(new GameDto(player.CardsInHand, player.CardsThisRound, _gameService.GetGamePhase(), _gameService.GetPromptCard().ToDto()));
    }

    [HttpGet("Phase")]
    public ActionResult<EGamePhase> GetPhase() =>
        Ok(_gameService.GetGamePhase());
}
