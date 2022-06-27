namespace Server.Models.Entities;

public record Player(Guid Id, string Name)
{
    public PlayerDto ToDto() => new(Id, Name);
}
