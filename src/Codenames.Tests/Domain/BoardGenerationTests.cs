using Codenames.Core.Domain.Generation;
using Codenames.Core.Domain.Words;
using Codenames.Core.Domain.Enums;
using Codenames.Core.Domain;

namespace Codenames.Tests.Domain;

public class BoardGenerationTests
{
    private static BoardGenerator CreateGenerator() => new BoardGenerator(new DefaultWordProvider());

    [Theory]
    [InlineData(Team.Red, 9, 8)]
    [InlineData(Team.Blue, 8, 9)]
    public void Board_HasExpectedDistribution(Team startingTeam, int expectedStartingAgents, int expectedOtherAgents)
    {
        var gen = CreateGenerator();
        var board = gen.Generate(startingTeam, seed: 12345);

        int red = board.All.Count(c => c.Type == CardType.RedAgent);
        int blue = board.All.Count(c => c.Type == CardType.BlueAgent);
        int neutral = board.All.Count(c => c.Type == CardType.Neutral);
        int assassin = board.All.Count(c => c.Type == CardType.Assassin);

        Assert.Equal(7, neutral);
        Assert.Equal(1, assassin);
        if (startingTeam == Team.Red)
        {
            Assert.Equal(expectedStartingAgents, red);
            Assert.Equal(expectedOtherAgents, blue);
        }
        else
        {
            Assert.Equal(expectedStartingAgents, blue);
            Assert.Equal(expectedOtherAgents, red);
        }
    }

    [Fact]
    public void Board_Words_AreUnique_CaseInsensitive()
    {
        var gen = CreateGenerator();
        var board = gen.Generate(Team.Red, seed: 42);
        var words = board.All.Select(c => c.Word.ToUpperInvariant()).ToList();
        Assert.Equal(words.Count, words.Distinct().Count());
    }

    [Fact]
    public void Board_Deterministic_WithSameSeed()
    {
        var gen1 = CreateGenerator();
        var gen2 = CreateGenerator();

        var boardA = gen1.Generate(Team.Red, seed: 999);
        var boardB = gen2.Generate(Team.Red, seed: 999);

        for (int i = 0; i < GameBoard.Size; i++)
        {
            Assert.Equal(boardA[i].Word, boardB[i].Word);
            Assert.Equal(boardA[i].Type, boardB[i].Type);
        }
    }
}
