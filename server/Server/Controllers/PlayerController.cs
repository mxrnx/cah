using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Enums;
using Server.Models.Dtos;
using Server.Models.Entities;
using Server.Services;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class PlayerController(CahContext context, IGameService gameService, ISessionService sessionService)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PlayerDto>> Post([FromBody] string name)
    {
        if (sessionService.GetCurrentPlayerId() is not null)
            return BadRequest("Current connection already tied to a session.");

        if (gameService.GetGamePhase() == EGamePhase.PickingAnswers)
            return BadRequest("Cannot join a game that has already started."); // TODO: consider making this possible
        
        if (name.Length is < 1 or > 20)
            return BadRequest("Name too long or short.");
        
        if (await context.Players.FirstOrDefaultAsync(x => x.Name == name) is not null)
            return BadRequest("Name already taken.");

        var newPlayer = new Player(Guid.NewGuid(), name);
        if (!await context.Players.AnyAsync())
            gameService.SetCzar(newPlayer.Id);

        // Add player to database
        context.Players.Add(newPlayer);
        await context.SaveChangesAsync();
        
        // Save player Id in a new session
        sessionService.CreateSession(newPlayer.Id);
        
        return Ok(newPlayer.ToDto(gameService.GetCzar() == newPlayer.Id));
    }
    
    [HttpDelete]
    public async Task<ActionResult> Delete([FromBody] Guid id)
    {
        var player = await context.Players.FindAsync(id);

        if (player is null)
            return BadRequest("No such player.");

        context.Players.Remove(player);
        sessionService.RemoveSession();
        await context.SaveChangesAsync();

        return NoContent();
    }
    
    [HttpGet]
    public async Task<ActionResult<List<PlayerDto>>> Get() =>
        await context.Players.Select(p => p.ToDto(gameService.GetCzar() == p.Id)).ToListAsync();
    
    /// <summary>
    /// Endpoint called when the client starts up. If the caller was a logged in player before (presumably) refreshing,
    /// we return that player; if not, we return an empty body.
    /// </summary>
    [HttpGet("Me")]
    public async Task<ActionResult<PlayerDto?>> GetMe()
    {
        var id = sessionService.GetCurrentPlayerId();
        
        if (id is null)
            return Ok("null");
        
        var player = await context.Players.FindAsync(id);

        if (player is null)
            throw new InvalidOperationException("Player exists according to session state but not in database");

        return Ok(player.ToDto(gameService.GetCzar() == player.Id));
    }
}
