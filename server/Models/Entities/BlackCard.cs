using System.Text.RegularExpressions;

namespace Server.Models.Entities;

public partial record BlackCard : Card
{
    private int FieldCount;
    
    public BlackCard(string text) : base(text)
    {
        var fieldCount = FieldRegex().Matches(text).Count;
        if (fieldCount < 1)
            throw new InvalidOperationException("Every black card needs at least 1 field.");
        
        FieldCount = fieldCount;
    }

    /// <summary>
    /// Cached regex for player-fillable fields.
    /// </summary>
    [GeneratedRegex("___")]
    private static partial Regex FieldRegex();
}
