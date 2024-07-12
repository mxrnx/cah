using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Enums;
using Server.Models.Requests;
using Server.Services;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class RoundController(CahContext context, IGameService gameService, IPlayerService playerService) : ControllerBase
{
    [HttpPost("PlayCard")]
    public async Task<ActionResult> PlayCard([FromBody] PlayCardRequest request)
    {
        if (gameService.GetGamePhase() != EGamePhase.PickingAnswers)
            return BadRequest("Wrong game phase.");

        var currentPlayer = await playerService.GetBySecretAsync(request.Secret);
        if (currentPlayer is null)
            return Unauthorized("Player not logged in when playing card.");
        
        if (currentPlayer.Id == gameService.GetCzar())
            return Unauthorized("Player is the Card Czar.");

        if (currentPlayer.CardsThisRound.Count >= gameService.GetPromptCard().FieldCount)
            return BadRequest("Player already played maximum amount of cards.");

        var card = await context.AnswerCards.SingleAsync(x => x.Id == request.CardId);
        currentPlayer.CardsThisRound.Add(card.Id);
        currentPlayer.CardsInHand.Remove(card.Id);
        await context.SaveChangesAsync();

        if (context.Players.Any(p => p.CardsThisRound.Count != gameService.GetPromptCard().FieldCount))
            return NoContent(); // Round is not finished yet, so we're done here

        // TODO: go to next phase
        
        return NoContent();
    }
}
