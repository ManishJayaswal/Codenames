using Codenames.Core.Domain.Services;
using Codenames.Core.Domain.Generation;
using Codenames.Core.Domain.Words;
using Codenames.Core.Domain.Enums;

namespace Codenames.Tests.Domain;

public class GameServiceTests
{
    private GameService CreateService() => new GameService(new BoardGenerator(new DefaultWordProvider()));

    [Theory]
    [InlineData(Team.Red)]
    [InlineData(Team.Blue)]
    public void CreateNewGame_SetsPhaseAndCounts(Team startingTeam)
    {
        var svc = CreateService();
        var game = svc.CreateNewGame(startingTeam, seed: 555);

        Assert.Equal(GamePhase.AwaitingClue, game.Phase);
        Assert.Equal(startingTeam, game.StartingTeam);
        Assert.Equal(startingTeam, game.CurrentTeam);
        // Distribution verification
        int red = game.Board.All.Count(c => c.Type == CardType.RedAgent);
        int blue = game.Board.All.Count(c => c.Type == CardType.BlueAgent);
        int neutral = game.Board.All.Count(c => c.Type == CardType.Neutral);
        int assassin = game.Board.All.Count(c => c.Type == CardType.Assassin);

        Assert.Equal(red, game.RedAgentsRemaining);
        Assert.Equal(blue, game.BlueAgentsRemaining);
        Assert.Equal(7, neutral);
        Assert.Equal(1, assassin);
        Assert.Equal(25, game.Board.All.Count);
    }

    [Fact]
    public void CreateNewGame_DeterministicBoardWithSeed()
    {
        var svc = CreateService();
        var g1 = svc.CreateNewGame(Team.Red, seed: 9999);
        var g2 = svc.CreateNewGame(Team.Red, seed: 9999);

        for (int i = 0; i < g1.Board.All.Count; i++)
        {
            Assert.Equal(g1.Board[i].Word, g2.Board[i].Word);
            Assert.Equal(g1.Board[i].Type, g2.Board[i].Type);
        }
    }
}
