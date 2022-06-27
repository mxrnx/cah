using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Models;
using Server.Models.Entities;
using Server.Services;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class PlayerController : ControllerBase
{
    private readonly CahContext _context;
    private readonly MemoryService _memoryService;

    public PlayerController(CahContext context, MemoryService memoryService)
    {
        _context = context;
        _memoryService = memoryService;
    }

    [HttpPost]
    public async Task<ActionResult<PlayerDto>> Post([FromBody] string name)
    {
        // TODO: check if player is not yet logged in/has no session
        if (name.Length is < 1 or > 20)
            return BadRequest("Name too long or short.");
        
        if (await _context.Players.FirstOrDefaultAsync(x => x.Name == name) is not null)
            return BadRequest("Name already taken.");

        var newPlayer = new Player(Guid.NewGuid(), name);
        if (!await _context.Players.AnyAsync())
            _memoryService.SetCzar(newPlayer.Id);

        _context.Players.Add(newPlayer);
        await _context.SaveChangesAsync();
        
        return Ok(newPlayer.ToDto());
    }
    
    [HttpDelete]
    public async Task<ActionResult<Guid>> Delete([FromBody] Guid id)
    {
        // TODO: check if player is not yet logged in/has no session
        var player = await _context.Players.FindAsync(id);

        if (player is null)
            return BadRequest("No such player.");

        _context.Players.Remove(player);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    [HttpGet]
    public async Task<ActionResult<List<PlayerDto>>> Get() =>
        await _context.Players.Select(p => p.ToDto()).ToListAsync();
}
