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
        var redPositions = game.Board.All.Select((c, i) => (c, i)).Where(t => t.c.Type == CardType.RedAgent).Select(t => t.i).ToList();
        var bluePositions = game.Board.All.Select((c, i) => (c, i)).Where(t => t.c.Type == CardType.BlueAgent).Select(t => t.i).ToList();

        // Red needs to find 9 agents, Blue needs 8. With guess limits, either team can win.
        GuessResult? last = null;
        int redIndex = 0;
        int blueIndex = 0;

        while (game.Phase != GamePhase.Complete)
        {
            // Handle Red's turn
            if (game.CurrentTeam == Team.Red && game.Phase == GamePhase.AwaitingClue)
            {
                svc.SubmitClue(game, Team.Red, "RED", 3);
            }

            if (game.CurrentTeam == Team.Red && game.Phase == GamePhase.AwaitingGuesses)
            {
                int guessesThisTurn = 0;
                int maxGuesses = 4; // 3 declared + 1 bonus

                while (guessesThisTurn < maxGuesses && game.Phase == GamePhase.AwaitingGuesses && redIndex < redPositions.Count)
                {
                    last = svc.MakeGuess(game, Team.Red, redPositions[redIndex++]);
                    guessesThisTurn++;

                    if (last.GameEnded) break;
                    if (last.TurnEnds) break;
                }
            }

            if (game.Phase == GamePhase.Complete) break;

            // Handle Blue's turn
            if (game.CurrentTeam == Team.Blue && game.Phase == GamePhase.AwaitingClue)
            {
                svc.SubmitClue(game, Team.Blue, "BLUE", 3);
            }

            if (game.CurrentTeam == Team.Blue && game.Phase == GamePhase.AwaitingGuesses)
            {
                int guessesThisTurn = 0;
                int maxGuesses = 4;

                while (guessesThisTurn < maxGuesses && game.Phase == GamePhase.AwaitingGuesses && blueIndex < bluePositions.Count)
                {
                    last = svc.MakeGuess(game, Team.Blue, bluePositions[blueIndex++]);
                    guessesThisTurn++;

                    if (last.GameEnded) break;
                    if (last.TurnEnds) break;
                }
            }
        }

        // Assert that the game completed with a winner (either team)
        Assert.NotNull(last);
        Assert.Equal(GamePhase.Complete, game.Phase);
        Assert.True(last!.GameEnded);
        Assert.NotNull(game.Winner);
        Assert.NotNull(last.Winner);
        Assert.Equal(game.Winner, last.Winner);

        // Verify the winner found all their agents
        if (game.Winner == Team.Red)
        {
            Assert.Equal(0, game.RedAgentsRemaining);
        }
        else
        {
            Assert.Equal(0, game.BlueAgentsRemaining);
        }
    }
}
