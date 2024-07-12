using Microsoft.EntityFrameworkCore;
using Server.Models.Entities;
using Server.Services;

namespace Server;

public class CahContext(DbContextOptions<CahContext> options, ICardParseService cardParseService) : DbContext(options)
{
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
        var decks = cardParseService.ParseDecks().ToArray(); // Convert to array to avoid multiple enumeration
        modelBuilder.Entity<Deck>()
            .HasData(decks);

        modelBuilder.Entity<PromptCard>()
            .HasData(cardParseService.ParsePromptCards(decks));
        modelBuilder.Entity<AnswerCard>()
            .HasData(cardParseService.ParseAnswerCards(decks));
    }

    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<PromptCard> PromptCards { get; set; } = null!;
    public DbSet<AnswerCard> AnswerCards { get; set; } = null!;
    public DbSet<Deck> Decks { get; set; } = null!;

}
