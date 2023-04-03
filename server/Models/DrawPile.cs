using Server.Extensions;
using Server.Models.Entities;

namespace Server.Models;

public class DrawPile<TCard> where TCard : Card
{
    private readonly TCard[] _fullPile;
    private Queue<TCard> _currentPile = new Queue<TCard>();

    public DrawPile(IEnumerable<TCard> allCards)
    {
        _fullPile = allCards.ToArray();
        ShufflePile();
    }

    private void ShufflePile()
    {
        _fullPile.Shuffle();
        _currentPile = new Queue<TCard>(_fullPile);
    }

    public TCard DrawCard() =>
        DrawCards(1).First();

    public IEnumerable<TCard> DrawCards(int count)
    {
        var drawnCards = new List<TCard>();

        while (count > 0)
        {
            if (_currentPile.TryDequeue(out var drawnCard))
            {
                drawnCards.Add(drawnCard);
                count--;
            }
            else
            {
                ShufflePile();
            }
            
        }

        return drawnCards;
    }
}
