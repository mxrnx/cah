using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class PlayerController : ControllerBase
{
    private readonly CahContext _context;

    public PlayerController(CahContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Post([FromBody] string name)
    {
        // TODO: check if player is not yet logged in/has no session
        // TODO: verify name validity
        if (name.Length is < 1 or > 20)
            return BadRequest("Name too long or short.");
        
        if (await _context.Players.FirstOrDefaultAsync(x => x.Name == name) is not null)
            return BadRequest("Name already taken.");
        
        var newPlayer = new Player(Guid.NewGuid(), name);
        _context.Players.Add(newPlayer);
        await _context.SaveChangesAsync();
        
        return Ok(newPlayer.Id);
    }
    
    [HttpDelete]
    public async Task<ActionResult<Guid>> Delete([FromBody] Guid id)
    {
        // TODO: check if player is not yet logged in/has no session
        var item = await _context.Players.FindAsync(id);

        if (item is null)
            return BadRequest("No such player.");

        _context.Players.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    [HttpGet]
    public async Task<ActionResult<List<Player>>> Get() =>
        await _context.Players.ToListAsync();

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Player>> Get(Guid id)
    {
        var item = await _context.Players.FindAsync(id);

        return item is null
            ? NotFound("No such player.")
            : item;
    }
}
