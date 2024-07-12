using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Enums;
using Server.Services;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class RoundController(CahContext context, IGameService gameService, ISessionService sessionService)
    : ControllerBase
{
    [HttpPost("PlayCard")]
    public async Task<ActionResult> PlayCard([FromBody] Guid cardId)
    {
        if (gameService.GetGamePhase() != EGamePhase.PickingAnswers)
            return BadRequest("Wrong game phase.");
        
        var currentPlayerId = sessionService.GetCurrentPlayerId();
        if (currentPlayerId is null)
            return Unauthorized("Player not logged in.");
        
        if (currentPlayerId == gameService.GetCzar())
            return Unauthorized("Player is the Card Czar.");

        var currentPlayer = await context.Players.SingleAsync(x => x.Id == currentPlayerId);
        if (currentPlayer.CardsThisRound.Count >= gameService.GetPromptCard().FieldCount)
            return BadRequest("Player already played maximum amount of cards.");

        var card = await context.AnswerCards.SingleAsync(x => x.Id == cardId);
        currentPlayer.CardsThisRound.Add(card.Id);
        currentPlayer.CardsInHand.Remove(card.Id);
        await context.SaveChangesAsync();

        if (context.Players.Any(p => p.CardsThisRound.Count != gameService.GetPromptCard().FieldCount))
            return NoContent(); // Round is not finished yet, so we're done here

        // TODO: go to next phase
        
        return NoContent();
    }
}
