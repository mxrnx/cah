using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Enums;
using Server.Extensions;
using Server.Models.Dtos;
using Server.Services;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class GameController(
    CahContext context,
    IGameService gameService,
    ISessionService sessionService,
    ICardService cardService) : ControllerBase
{
    private const int CardsInFullHand = 8;

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] int necessaryWins)
    {
        if (gameService.GetGamePhase() != EGamePhase.WaitingToStart)
            return BadRequest("Game has already started.");
        
        var currentPlayerId = sessionService.GetCurrentPlayerId();
        if (currentPlayerId is null)
            return Unauthorized("Player not logged in.");
        
        if (currentPlayerId != gameService.GetCzar())
            return Unauthorized("Player is not the Card Czar.");
        
        if (necessaryWins is < 1 or > 20)
            return BadRequest("Number of necessary wins must be greater than 1 and less than 20.");

        // TODO: make it possible to select decks
        gameService.SetupGame(necessaryWins, context.Decks.Include(x => x.AnswerCards).Include(x => x.PromptCards));

        foreach (var player in context.Players)
            player.CardsInHand.AddMany(gameService.DrawAnswerCards(CardsInFullHand).Select(x => x.Id));

        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<GameDto>> Get()
    {
        var player = await context.Players.SingleOrDefaultAsync(x => x.Id == sessionService.GetCurrentPlayerId());
        if (player is null)
            return Unauthorized("Player not logged in.");

        return Ok(new GameDto(player.CardsInHand.Select(cardService.GetAnswerCard),
            player.CardsThisRound.Select(cardService.GetAnswerCard), gameService.GetGamePhase(),
            gameService.GetPromptCard().ToDto()));
    }

    [HttpGet("Phase")]
    public ActionResult<EGamePhase> GetPhase() =>
        Ok(gameService.GetGamePhase());
}
