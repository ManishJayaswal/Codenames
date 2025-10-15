using Codenames.Core.Domain;
using Codenames.Core.Domain.Enums;

namespace Codenames.Tests.Domain;

public class GameBoardTests
{
    [Fact]
    public void GameBoard_HasExpectedSize()
    {
        var board = GameBoard.CreateEmpty();
        Assert.Equal(GameBoard.Size, board.All.Count);
        Assert.Equal(25, GameBoard.Size);
        Assert.Equal(string.Empty, board[0].Word);
    }

    [Fact]
    public void GameBoard_Indexer_ReturnsCardAtPosition()
    {
        var cards = Enumerable.Range(0, GameBoard.Size)
            .Select(i => new GameCard(i, $"Word{i}", CardType.Neutral))
            .ToList();
        var board = GameBoard.Create(cards);

        Assert.Equal("Word0", board[0].Word);
        Assert.Equal($"Word{GameBoard.Size - 1}", board[GameBoard.Size - 1].Word);
    }

    [Fact]
    public void GameBoard_Indexer_OutOfRangeLow_Throws()
    {
        var board = GameBoard.CreateEmpty();
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = board[-1]);
    }

    [Fact]
    public void GameBoard_Indexer_OutOfRangeHigh_Throws()
    {
        var board = GameBoard.CreateEmpty();
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = board[GameBoard.Size]);
    }

    [Fact]
    public void GameBoard_ConstructingWithWrongCount_Throws()
    {
        var cards = Enumerable.Range(0, 5)
            .Select(i => new GameCard(i, $"W{i}", CardType.Neutral))
            .ToList();
        Assert.Throws<ArgumentException>(() => GameBoard.Create(cards));
    }
}
