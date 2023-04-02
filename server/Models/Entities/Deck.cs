namespace Server.Models.Entities;

public record Deck
{
    private string Name;
    private List<BlackCard> _blackCards;
    private List<WhiteCard> _whiteCards;
    
    public Deck(string name, List<BlackCard> blackCards, List<WhiteCard> whiteCards)
    {
        Name = name;
        _blackCards = blackCards;
        _whiteCards = whiteCards;
    }
}
