using Codenames.Core.Domain;
using Codenames.Core.Domain.Enums;

namespace Codenames.Tests.Domain;

public class GameTests
{
    [Fact]
    public void Game_CreateNew_SetsDefaults()
    {
        var game = Game.CreateNew(startingTeam: Team.Blue);
        Assert.NotEqual(Guid.Empty, game.Id);
        Assert.Equal(Team.Blue, game.StartingTeam);
        Assert.Equal(Team.Blue, game.CurrentTeam);
        Assert.Equal(GamePhase.Setup, game.Phase);
        Assert.Empty(game.ClueHistory);
        Assert.Equal(25, game.Board.All.Count);
    }
}
