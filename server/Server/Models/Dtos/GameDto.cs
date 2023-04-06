using JetBrains.Annotations;
using Server.Enums;
using Server.Models.Entities;

namespace Server.Models.Dtos;

[PublicAPI]
public record GameDto(
    IEnumerable<AnswerCard> HandCards,
    IEnumerable<AnswerCard> RoundCards,
    EGamePhase GamePhase,
    PromptCardDto PromptCard);
