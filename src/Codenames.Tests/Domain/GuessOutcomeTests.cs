using Codenames.Core.Domain.Enums;
using Codenames.Core.Domain.Generation;
using Codenames.Core.Domain.Services;
using Codenames.Core.Domain.Words;
using Codenames.Core.Domain.Guesses;
using Codenames.Core.Domain.Exceptions;

namespace Codenames.Tests.Domain;

public class GuessOutcomeTests
{
    private GameService CreateService() => new GameService(new BoardGenerator(new DefaultWordProvider()));

    private (GameService svc, Codenames.Core.Domain.Game game) CreateGame(Team startingTeam = Team.Red, int seed = 1234)
    {
        var svc = CreateService();
        var game = svc.CreateNewGame(startingTeam, seed);
        // Submit a valid clue to move into guessing phase
        svc.SubmitClue(game, startingTeam, "ALPHA", 3);
        return (svc, game);
    }

    [Fact]
    public void Guess_FriendlyAgent_StaysInGuessingPhase()
    {
        var (svc, game) = CreateGame(Team.Red, 200);
        // Find first red agent index
    int pos = game.Board.All.Select((c,i)=> (c,i)).First(t => t.c.Type == CardType.RedAgent).i;
        var result = svc.MakeGuess(game, Team.Red, pos);
        Assert.Equal(GuessOutcome.FriendlyAgent, result.Outcome);
        Assert.False(result.TurnEnds);
        Assert.Equal(GamePhase.AwaitingGuesses, game.Phase);
    }

    [Fact]
    public void Guess_OpponentAgent_TurnEnds_SwitchesTeam()
    {
        var (svc, game) = CreateGame(Team.Red, 201);
    int pos = game.Board.All.Select((c,i)=> (c,i)).First(t => t.c.Type == CardType.BlueAgent).i;
        var result = svc.MakeGuess(game, Team.Red, pos);
        Assert.Equal(GuessOutcome.OpponentAgent, result.Outcome);
        Assert.True(result.TurnEnds);
        Assert.Equal(GamePhase.AwaitingClue, game.Phase);
        Assert.Equal(Team.Blue, game.CurrentTeam);
    }

    [Fact]
    public void Guess_Neutral_TurnEnds_SwitchesTeam()
    {
        var (svc, game) = CreateGame(Team.Red, 202);
    int pos = game.Board.All.Select((c,i)=> (c,i)).First(t => t.c.Type == CardType.Neutral).i;
        var result = svc.MakeGuess(game, Team.Red, pos);
        Assert.Equal(GuessOutcome.Neutral, result.Outcome);
        Assert.True(result.TurnEnds);
        Assert.Equal(GamePhase.AwaitingClue, game.Phase);
        Assert.Equal(Team.Blue, game.CurrentTeam);
    }

    [Fact]
    public void Guess_Assassin_GameEnds_OpponentWins()
    {
        var (svc, game) = CreateGame(Team.Red, 203);
    int pos = game.Board.All.Select((c,i)=> (c,i)).First(t => t.c.Type == CardType.Assassin).i;
        var result = svc.MakeGuess(game, Team.Red, pos);
        Assert.Equal(GuessOutcome.Assassin, result.Outcome);
        Assert.True(result.GameEnded);
        Assert.Equal(GamePhase.Complete, game.Phase);
        Assert.Equal(Team.Blue, result.Winner);
        Assert.Equal(Team.Blue, game.Winner);
    }

    [Fact]
    public void Guess_FriendlyAgent_WinCompletesGame()
    {
        var (svc, game) = CreateGame(Team.Red, 204);
        var redPositions = game.Board.All.Select((c,i)=> (c,i)).Where(t => t.c.Type == CardType.RedAgent).Select(t=>t.i).ToList();

        // Sequentially guess red agent positions until game ends.
        GuessResult? last = null;
        foreach (var pos in redPositions)
        {
            last = svc.MakeGuess(game, Team.Red, pos);
            if (game.Phase == GamePhase.Complete) break; // win achieved
            // If turn ended unexpectedly (shouldn't on friendly) fail early
            Assert.False(last.TurnEnds && !last.GameEnded, "Turn ended prematurely on friendly agent guess");
        }

        Assert.NotNull(last);
        Assert.Equal(GamePhase.Complete, game.Phase);
        Assert.True(last!.GameEnded);
        Assert.Equal(Team.Red, game.Winner);
        Assert.Equal(Team.Red, last.Winner);
    }
}
