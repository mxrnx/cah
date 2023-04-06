using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Enums;
using Server.Services;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class RoundController : ControllerBase
{
    private readonly CahContext _context;
    private readonly GameService _gameService;
    private readonly SessionService _sessionService;

    public RoundController(CahContext context, GameService gameService, SessionService sessionService)
    {
        _context = context;
        _gameService = gameService;
        _sessionService = sessionService;
    }
    
    [HttpPost("PlayCard")]
    public async Task<ActionResult> PlayCard([FromBody] Guid cardId)
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
}
