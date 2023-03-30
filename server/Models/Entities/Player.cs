using Server.Models.Dtos;

namespace Server.Models.Entities;

public record Player(Guid Id, string Name)
{
    public PlayerDto ToDto(bool isCzar) => new(Id, Name, isCzar);
}
