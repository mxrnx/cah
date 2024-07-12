using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Server.Models.Dtos;

namespace Server.Models.Entities;

public class Player(Guid id, string name)
{
    public Guid Id { get; init; } = id;
    
    [MaxLength(128)]
    public string Name { get; init; } = name;
    
    [NotMapped]
    public ICollection<Guid> CardsInHand { get; set; } = new List<Guid>();
    
    [NotMapped]
    public ICollection<Guid> CardsThisRound { get; set; } = new List<Guid>();
    
    public PlayerDto ToDto(bool isCzar) => new(Id, Name, isCzar);
}
