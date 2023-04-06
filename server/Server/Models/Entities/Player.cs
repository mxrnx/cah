using Server.Models.Dtos;

namespace Server.Models.Entities;

public class Player
{
    public Player(Guid id, string name)
    {
        Id = id;
        Name = name;
        CardsInHand = new List<AnswerCard>();
        CardsThisRound = new List<AnswerCard>();
    }
    
    public PlayerDto ToDto(bool isCzar) => new(Id, Name, isCzar);
    public Guid Id { get; init; }
    public string Name { get; init; }
    public ICollection<AnswerCard> CardsInHand { get; set; }
    public ICollection<AnswerCard> CardsThisRound { get; set; }
}
