using Server.Models.Entities;

namespace Server.Models.Dtos;

public record GameDto(IEnumerable<AnswerCard> HandCards);
