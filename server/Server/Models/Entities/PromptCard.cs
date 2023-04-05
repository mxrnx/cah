using System.Text.RegularExpressions;
using Server.Models.Dtos;

namespace Server.Models.Entities;

public partial class PromptCard : Card
{
    public int FieldCount
    {
        get
        {
            var fieldCount = FieldRegex().Matches(Text).Count;
            if (fieldCount < 1)
                throw new InvalidOperationException("Every prompt card needs at least 1 field.");
            return fieldCount;
        }
    }

    public PromptCardDto ToDto() => new(Id, Text, FieldCount);

    /// <summary>
    /// Cached regex for player-fillable fields.
    /// </summary>
    [GeneratedRegex("___")]
    private static partial Regex FieldRegex();
}
