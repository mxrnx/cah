using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Server.Models.Dtos;

namespace Server.Models.Entities;

public class Player
{
    public required Guid Id { get; init; }
    
    [MaxLength(128)]
    public required string Name { get; init; }
    
    [NotMapped]
    public ICollection<Guid> CardsInHand { get; set; } = new List<Guid>();
    
    [NotMapped]
    public ICollection<Guid> CardsThisRound { get; set; } = new List<Guid>();

    /// <summary>
    /// Secret value sent only to the client that created this player, used to keep track of sessions.
    /// </summary>
    public required Guid Secret { get; set; }

    public PlayerDto ToDto(bool isCzar) => new(Id, Name, isCzar, null);
    
    public PlayerDto ToDtoWithSecret(bool isCzar) => new(Id, Name, isCzar, Secret);
}
