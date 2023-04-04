namespace Server.Models.Entities;

public class Deck
{
    public Deck(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
    
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ICollection<PromptCard> PromptCards { get; set; } = null!;
    public ICollection<AnswerCard> AnswerCards { get; set; } = null!;
}
