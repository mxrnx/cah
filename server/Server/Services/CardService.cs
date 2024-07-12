using Server.Models.Entities;

namespace Server.Services;

public interface ICardService
{
    AnswerCard GetAnswerCard(Guid id);
}

public class CardService(CahContext context) : ICardService
{
    public AnswerCard GetAnswerCard(Guid id) => 
        context.AnswerCards.First(x => x.Id == id);
}