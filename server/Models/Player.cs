namespace Server.Models;

public readonly record struct Player(Guid Id, String Name);
public readonly record struct PlayerCreate(String Name);
