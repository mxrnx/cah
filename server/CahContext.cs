using Microsoft.EntityFrameworkCore;
using Server.Models.Entities;

namespace Server;

public class CahContext : DbContext
{
    public CahContext(DbContextOptions<CahContext> options) : base(options)
    {
    }

    public Guid CurrentCzar = Guid.Empty;
    public DbSet<Player> Players { get; set; } = null!;
}
