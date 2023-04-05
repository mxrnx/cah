using JetBrains.Annotations;
using Server.Enums;
using Server.Models.Entities;

namespace Server.Models.Dtos;

[PublicAPI]
public record GameDto(IEnumerable<AnswerCard> HandCards, EGamePhase GamePhase, PromptCardDto PromptCard);
