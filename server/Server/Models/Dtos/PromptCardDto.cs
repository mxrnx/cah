using JetBrains.Annotations;

namespace Server.Models.Dtos;

[PublicAPI]
public record PromptCardDto(Guid Id, string Text, int FieldCount);
