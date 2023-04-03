using System.Text.RegularExpressions;

namespace Server.Models.Entities;

public partial class PromptCard : Card
{
    public int FieldCount
    {
        get
        {
            var fieldCount = FieldRegex().Matches(Text).Count;
            if (fieldCount < 1)
                throw new InvalidOperationException("Every black card needs at least 1 field.");
            return fieldCount;
        }
    }

    /// <summary>
    /// Cached regex for player-fillable fields.
    /// </summary>
    [GeneratedRegex("___")]
    private static partial Regex FieldRegex();
}
