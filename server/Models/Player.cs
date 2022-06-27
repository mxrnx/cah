namespace Server.Models;

public record Player(Guid Id, string Name);
public record PlayerCreate(string Name);
