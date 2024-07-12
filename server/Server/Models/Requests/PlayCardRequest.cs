namespace Server.Models.Requests;

public record PlayCardRequest(Guid Secret, Guid CardId);