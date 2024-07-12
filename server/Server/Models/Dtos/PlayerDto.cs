namespace Server.Models.Dtos;

using JetBrains.Annotations;

[PublicAPI]
public record PlayerDto(Guid Id, string Name, bool Czar, Guid? Secret);
