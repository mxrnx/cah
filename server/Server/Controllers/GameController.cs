using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Enums;
using Server.Extensions;
using Server.Models.Dtos;
using Server.Models.Requests;
using Server.Services;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class GameController(
    CahContext context,
    IGameService gameService,
    IPlayerService playerService,
    ICardService cardService) : ControllerBase
{
    private const int CardsInFullHand = 8;

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] PostGameRequest request)
    {
        if (gameService.GetGamePhase() != EGamePhase.WaitingToStart)
            return BadRequest("Game has already started.");
        
        var currentPlayer = await playerService.GetBySecretAsync(request.Secret);
        if (currentPlayer is null)
            return Unauthorized("Player not logged in when starting game.");
        
        if (currentPlayer.Id != gameService.GetCzar())
            return Unauthorized("Player is not the Card Czar.");
        
        if (request.NecessaryWins is < 1 or > 20)
            return BadRequest("Number of necessary wins must be greater than 1 and less than 20.");

        // TODO: make it possible to select decks
        gameService.SetupGame(request.NecessaryWins, context.Decks.Include(x => x.AnswerCards).Include(x => x.PromptCards));

        foreach (var player in context.Players)
            player.CardsInHand.AddMany(gameService.DrawAnswerCards(CardsInFullHand).Select(x => x.Id));

        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<GameDto>> Get([FromQuery] Guid secret)
    {
        var player = await playerService.GetBySecretAsync(secret);
        if (player is null)
            return Unauthorized("Player not logged in during get.");

        return Ok(new GameDto(player.CardsInHand.Select(cardService.GetAnswerCard),
            player.CardsThisRound.Select(cardService.GetAnswerCard), gameService.GetGamePhase(),
            gameService.GetPromptCard().ToDto()));
    }

    [HttpGet("Phase")]
    public ActionResult<EGamePhase> GetPhase() =>
        Ok(gameService.GetGamePhase());
}
