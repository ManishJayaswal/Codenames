using Codenames.Core.Domain;
using Codenames.Core.Domain.Enums;

namespace Codenames.Tests.Domain;

public class ClueTests
{
    [Fact]
    public void Clue_ConstructValid_Succeeds()
    {
        var clue = new Clue("alpha", 2, Team.Red, new DateTime(2024,1,1,0,0,0, DateTimeKind.Utc));
        Assert.Equal("alpha", clue.Text);
        Assert.Equal(2, clue.DeclaredCount);
        Assert.Equal(Team.Red, clue.Team);
        Assert.Equal(new DateTime(2024,1,1,0,0,0, DateTimeKind.Utc), clue.Timestamp);
    }

    [Fact]
    public void Clue_NegativeDeclaredCount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Clue("alpha", -1, Team.Red));
    }

    [Fact]
    public void Clue_EmptyText_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Clue("", 1, Team.Red));
    }
}
