using FluentAssertions;
using Server.Models.Entities;
using Xunit;

namespace ServerTests.Models.Entities;

public class PromptCardTests
{
    [Fact]
    public void PromptCard_WithFields_ShouldReturnFieldCount()
    {
        var card = new PromptCard
        {
            Id = Guid.NewGuid(),
            Text = "This ___ has two fields, ___!"
        };

        card.FieldCount.Should().Be(2);
    }
    
    [Fact]
    public void PromptCard_WithNoFields_ShouldThrow()
    {
        var card = new PromptCard
        {
            Id = Guid.NewGuid(),
            Text = "This prompt is missing underscores (fields)."
        };

        card.Invoking(x => x.FieldCount).Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Every prompt card needs at least 1 field.");
    }
}
