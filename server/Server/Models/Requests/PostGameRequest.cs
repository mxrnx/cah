namespace Server.Models.Requests;

public record PostGameRequest(int NecessaryWins, Guid Secret);