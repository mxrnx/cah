using System.Text;
using Server.Models.Entities;

namespace Server.Services;

public interface ICardParseService
{
    /// <summary>
    /// Create new decks based on directory names
    /// </summary>
    IEnumerable<Deck> ParseDecks();

    IEnumerable<PromptCard> ParsePromptCards(Deck[] decks);

    IEnumerable<AnswerCard> ParseAnswerCards(Deck[] decks);
}

public class CardParseService : ICardParseService
{
    public IEnumerable<Deck> ParseDecks()
    {
        return GetDeckDirectories().Select(dir => new Deck(Guid.NewGuid(), dir.Name)).ToList();
    }

    public IEnumerable<PromptCard> ParsePromptCards(Deck[] decks) =>
        ParseCards<PromptCard>(decks, "prompts.txt");
    
    public IEnumerable<AnswerCard> ParseAnswerCards(Deck[] decks) =>
        ParseCards<AnswerCard>(decks, "answers.txt");

    private static IEnumerable<T> ParseCards<T>(Deck[] decks, string filename) where T : Card, new()
    {
        var cards = new List<T>();
        
        foreach (var deckDirectory in GetDeckDirectories())
        {
            var file = deckDirectory.GetFiles(filename).SingleOrDefault();
            if (file is null) // No cards of this type in this deck
                continue;
            
            var fileStream = file.OpenRead();
            using var streamReader = new StreamReader(fileStream, Encoding.UTF8);

            var deck = decks.First(x => x.Name == deckDirectory.Name);
            
            while (streamReader.ReadLine() is {} cardText)
            {
                var newCard = new T
                {
                    Id = Guid.NewGuid(),
                    Text = cardText,
                    DeckId = deck.Id
                };
                cards.Add(newCard);
            }
        }
        
        return cards;
    }

    /// <summary>
    /// Gets deck directories under /decks if they exist, throws otherwise
    /// </summary>
    /// <returns>List of DirectoryInfo objects for the decks under the decks directory</returns>
    /// <exception cref="InvalidOperationException">When the directory is not found</exception>
    private static IEnumerable<DirectoryInfo> GetDeckDirectories()
    {
        
        var cardsDirectoryPath = Directory.GetCurrentDirectory() + "/../../decks/";
        var cardsDirectoryInfo = new DirectoryInfo(cardsDirectoryPath);
        if (cardsDirectoryInfo is null || !cardsDirectoryInfo.Exists)
            throw new InvalidOperationException($"Path '{cardsDirectoryPath}' to cards does not exist. Please create it.");
        
        return cardsDirectoryInfo.GetDirectories();
    }
}
