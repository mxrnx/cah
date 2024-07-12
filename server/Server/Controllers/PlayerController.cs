using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Enums;
using Server.Models.Dtos;
using Server.Services;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class PlayerController(CahContext context, IGameService gameService, IPlayerService playerService)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PlayerDto>> Post([FromBody] string name)
    {
        if (gameService.GetGamePhase() == EGamePhase.PickingAnswers)
            return BadRequest("Cannot join a game that has already started."); // TODO: consider making this possible

        var (newPlayer, problem) = await playerService.CreateAsync(name);

        if (problem is not null)
            return BadRequest(problem);

        if (newPlayer is null)
            throw new InvalidOperationException("Player was null, somehow.");
        
        return Ok(newPlayer.ToDtoWithSecret(gameService.GetCzar() == newPlayer.Id));
    }
    
    [HttpDelete]
    public async Task<ActionResult> Delete([FromBody] Guid secret)
    {
        var success = await playerService.DeleteAsync(secret);

        return success ? NoContent() : BadRequest("Could not delete player");
    }
    
    [HttpGet]
    public async Task<ActionResult<List<PlayerDto>>> Get() =>
        await context.Players.Select(p => p.ToDto(gameService.GetCzar() == p.Id)).ToListAsync();
}
