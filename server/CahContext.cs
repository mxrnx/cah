using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server;

public class CahContext : DbContext
{
    public CahContext(DbContextOptions<CahContext> options) : base(options)
    {
    }

    public DbSet<Player> Players { get; set; } = null!;
}
