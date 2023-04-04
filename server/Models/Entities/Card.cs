namespace Server.Models.Entities;

public class Card
{
    protected Card()
    {
        Id = Guid.NewGuid();
        Text = string.Empty;
    }
    
    public Guid Id { get; set; }
    public string Text { get; set; }
    public Guid DeckId { get; set; } = Guid.Empty;
    public Deck Deck { get; set; } = null!;

}
