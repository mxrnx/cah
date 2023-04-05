using Microsoft.EntityFrameworkCore;
using Server.Models.Entities;
using Server.Services;

namespace Server;

public class CahContext : DbContext
{
    private readonly CardParseService _cardParseService;
    
    public CahContext(DbContextOptions<CahContext> options, CardParseService cardParseService) : base(options)
    {
        _cardParseService = cardParseService;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Set up one-to-many relations between deck and prompt/answer cards
        modelBuilder.Entity<Deck>()
            .HasMany<PromptCard>(x => x.PromptCards)
            .WithOne(x => x.Deck)
            .HasForeignKey(x => x.DeckId)
            .IsRequired();
        modelBuilder.Entity<Deck>()
            .HasMany<AnswerCard>(x => x.AnswerCards)
            .WithOne(x => x.Deck)
            .HasForeignKey(x => x.DeckId)
            .IsRequired();
        
        // Seed decks and prompt and answer cards
        var decks = _cardParseService.ParseDecks().ToArray(); // Convert to array to avoid multiple enumeration
        modelBuilder.Entity<Deck>()
            .HasData(decks);

        modelBuilder.Entity<PromptCard>()
            .HasData(_cardParseService.ParsePromptCards(decks));
        modelBuilder.Entity<AnswerCard>()
            .HasData(_cardParseService.ParseAnswerCards(decks));
    }

    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<PromptCard> PromptCards { get; set; } = null!;
    public DbSet<AnswerCard> AnswerCards { get; set; } = null!;
    public DbSet<Deck> Decks { get; set; } = null!;

}
