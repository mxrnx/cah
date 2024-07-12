using Microsoft.EntityFrameworkCore;
using Server.Models.Entities;

namespace Server.Services;

public interface IPlayerService
{
    Task<Player?> GetBySecretAsync(Guid secret);
    Task<(Player?, string?)> CreateAsync(string name);
    Task<bool> DeleteAsync(Guid secret);
}

public class PlayerService(CahContext context, IGameService gameService) : IPlayerService
{
    public Task<Player?> GetBySecretAsync(Guid secret)
    {
        return context.Players.FirstOrDefaultAsync(x => x.Secret == secret);
    }

    // TODO: refactor so we do not need the gameservice and have a nicer return type
    public async Task<(Player?, string?)> CreateAsync(string name)
    {
        if (name.Length is < 1 or > 20)
            return (null, "Name too long or short.");

        if (await context.Players.FirstOrDefaultAsync(x => x.Name == name) is not null)
            return (null, "Name already taken.");

        var newPlayer = new Player
        {
          Id = Guid.NewGuid(), 
          Name = name, 
          Secret = Guid.NewGuid()
        };
        if (!await context.Players.AnyAsync())
            gameService.SetCzar(newPlayer.Id);

        // Add player to database
        context.Players.Add(newPlayer);
        await context.SaveChangesAsync();

        return (newPlayer, null);
    }

    public async Task<bool> DeleteAsync(Guid secret)
    {
        var player = await GetBySecretAsync(secret);

        if (player is null)
            return false;

        context.Players.Remove(player);
        await context.SaveChangesAsync();

        return true;
    }
}