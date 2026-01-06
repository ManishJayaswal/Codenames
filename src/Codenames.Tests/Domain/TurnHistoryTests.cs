using Codenames.Core.Domain.Enums;
using Codenames.Core.Domain.Generation;
using Codenames.Core.Domain.Services;
using Codenames.Core.Domain.Words;
using Codenames.Core.Domain.Guesses;
using Codenames.Core.Domain.Turns;

namespace Codenames.Tests.Domain;

/// <summary>
/// Stage 7 tests: Turn history recording and win conditions with history verification.
/// </summary>
public class TurnHistoryTests
{
    private GameService CreateService() => new GameService(new BoardGenerator(new DefaultWordProvider()));

    [Fact]
    public void TurnHistory_EmptyAtGameStart()
    {
        var svc = CreateService();
        var game = svc.CreateNewGame(Team.Red, seed: 7000);

        Assert.Empty(game.TurnHistory);
        Assert.Null(game.CurrentTurn);
    }

    [Fact]
    public void CurrentTurn_CreatedOnClueSubmission()
    {
        var svc = CreateService();
        var game = svc.CreateNewGame(Team.Red, seed: 7001);
        
        svc.SubmitClue(game, Team.Red, "ALPHA", 2);

        Assert.NotNull(game.CurrentTurn);
        Assert.Equal(Team.Red, game.CurrentTurn!.Team);
        Assert.Equal("ALPHA", game.CurrentTurn.Clue.Text);
        Assert.Equal(2, game.CurrentTurn.Clue.DeclaredCount);
        Assert.Empty(game.CurrentTurn.Guesses);
    }

    [Fact]
    public void GuessEvent_RecordedInCurrentTurn()
    {
        var svc = CreateService();
        var game = svc.CreateNewGame(Team.Red, seed: 7002);
        svc.SubmitClue(game, Team.Red, "BETA", 1);

        var redPos = game.Board.All.Select((c, i) => (c, i))
            .First(t => t.c.Type == CardType.RedAgent).i;
        
        svc.MakeGuess(game, Team.Red, redPos);

        Assert.NotNull(game.CurrentTurn);
        Assert.Single(game.CurrentTurn!.Guesses);
        var guessEvent = game.CurrentTurn.Guesses[0];
        Assert.Equal(redPos, guessEvent.Position);
        Assert.Equal(GuessOutcome.FriendlyAgent, guessEvent.Outcome);
        Assert.Equal(CardType.RedAgent, guessEvent.CardType);
    }

    [Fact]
    public void TurnHistory_RecordedWhenTurnEndsOnWrongGuess()
    {
        var svc = CreateService();
        var game = svc.CreateNewGame(Team.Red, seed: 7003);
        svc.SubmitClue(game, Team.Red, "GAMMA", 2);

        var neutralPos = game.Board.All.Select((c, i) => (c, i))
            .First(t => t.c.Type == CardType.Neutral).i;
        
        svc.MakeGuess(game, Team.Red, neutralPos);

        // Turn should end and be recorded
        Assert.Null(game.CurrentTurn);
        Assert.Single(game.TurnHistory);
        
        var turn = game.TurnHistory[0];
        Assert.Equal(Team.Red, turn.Team);
        Assert.Equal("GAMMA", turn.Clue.Text);
        Assert.Single(turn.Guesses);
        Assert.False(turn.TurnEndedVoluntarily);
        Assert.False(turn.GameEnded);
        Assert.Null(turn.Winner);
    }

    [Fact]
    public void TurnHistory_RecordedWhenTurnEndsVoluntarily()
    {
        var svc = CreateService();
        var game = svc.CreateNewGame(Team.Red, seed: 7004);
        svc.SubmitClue(game, Team.Red, "DELTA", 3);

        var redPos = game.Board.All.Select((c, i) => (c, i))
            .First(t => t.c.Type == CardType.RedAgent).i;
        
        svc.MakeGuess(game, Team.Red, redPos);
        svc.EndTurn(game, Team.Red);

        Assert.Null(game.CurrentTurn);
        Assert.Single(game.TurnHistory);
        
        var turn = game.TurnHistory[0];
        Assert.Equal(Team.Red, turn.Team);
        Assert.Single(turn.Guesses);
        Assert.True(turn.TurnEndedVoluntarily);
        Assert.False(turn.GameEnded);
        Assert.Null(turn.Winner);
    }

    [Fact]
    public void TurnHistory_MultipleGuessesRecorded()
    {
        var svc = CreateService();
        var game = svc.CreateNewGame(Team.Red, seed: 7005);
        svc.SubmitClue(game, Team.Red, "EPSILON", 2);

        var redPositions = game.Board.All.Select((c, i) => (c, i))
            .Where(t => t.c.Type == CardType.RedAgent)
            .Select(t => t.i)
            .Take(2)
            .ToList();
        
        svc.MakeGuess(game, Team.Red, redPositions[0]);
        svc.MakeGuess(game, Team.Red, redPositions[1]);

        Assert.NotNull(game.CurrentTurn);
        Assert.Equal(2, game.CurrentTurn!.Guesses.Count);
        Assert.All(game.CurrentTurn.Guesses, g => Assert.Equal(GuessOutcome.FriendlyAgent, g.Outcome));
    }

    [Fact]
    public void TurnHistory_GuessLimitEndsAndRecordsTurn()
    {
        var svc = CreateService();
        var game = svc.CreateNewGame(Team.Red, seed: 7006);
        svc.SubmitClue(game, Team.Red, "ZETA", 1); // Max 2 guesses

        var redPositions = game.Board.All.Select((c, i) => (c, i))
            .Where(t => t.c.Type == CardType.RedAgent)
            .Select(t => t.i)
            .Take(2)
            .ToList();
        
        svc.MakeGuess(game, Team.Red, redPositions[0]);
        svc.MakeGuess(game, Team.Red, redPositions[1]); // Should end turn

        Assert.Null(game.CurrentTurn);
        Assert.Single(game.TurnHistory);
        
        var turn = game.TurnHistory[0];
        Assert.Equal(2, turn.Guesses.Count);
        Assert.False(turn.TurnEndedVoluntarily);
    }

    [Fact]
    public void TurnHistory_AssassinEndsGameAndRecordsTurn()
    {
        var svc = CreateService();
        var game = svc.CreateNewGame(Team.Red, seed: 7007);
        svc.SubmitClue(game, Team.Red, "ETA", 1);

        var assassinPos = game.Board.All.Select((c, i) => (c, i))
            .First(t => t.c.Type == CardType.Assassin).i;
        
        svc.MakeGuess(game, Team.Red, assassinPos);

        Assert.Null(game.CurrentTurn);
        Assert.Single(game.TurnHistory);
        Assert.Equal(GamePhase.Complete, game.Phase);
        
        var turn = game.TurnHistory[0];
        Assert.Equal(Team.Red, turn.Team);
        Assert.Single(turn.Guesses);
        Assert.Equal(GuessOutcome.Assassin, turn.Guesses[0].Outcome);
        Assert.True(turn.GameEnded);
        Assert.Equal(Team.Blue, turn.Winner);
        Assert.Equal(Team.Blue, game.Winner);
    }

    [Fact]
    public void TurnHistory_WinByAgentsEndsGameAndRecordsTurn()
    {
        var svc = CreateService();
        var game = svc.CreateNewGame(Team.Red, seed: 7008);

        // Red team has 9 agents to find, Blue has 8
        var redPositions = game.Board.All.Select((c, i) => (c, i))
            .Where(t => t.c.Type == CardType.RedAgent)
            .Select(t => t.i)
            .ToList();

        Assert.Equal(9, redPositions.Count);

        // Guess all but one red agent
        for (int i = 0; i < 8; i++)
        {
            if (game.CurrentTeam != Team.Red || game.Phase != GamePhase.AwaitingClue)
            {
                // Handle Blue's turn if it happens
                if (game.CurrentTeam == Team.Blue && game.Phase == GamePhase.AwaitingClue)
                {
                    svc.SubmitClue(game, Team.Blue, "SKIP", 0);
                    svc.EndTurn(game, Team.Blue);
                }
                
                if (game.CurrentTeam == Team.Red && game.Phase == GamePhase.AwaitingClue)
                {
                    svc.SubmitClue(game, Team.Red, "REDWORD", 3);
                }
            }
            else
            {
                svc.SubmitClue(game, Team.Red, "REDWORD", 3);
            }
            
            if (game.Phase == GamePhase.AwaitingGuesses && game.CurrentTeam == Team.Red)
            {
                svc.MakeGuess(game, Team.Red, redPositions[i]);
                if (game.Phase == GamePhase.AwaitingGuesses)
                {
                    svc.EndTurn(game, Team.Red);
                }
            }
        }

        // Now guess the final red agent to win
        if (game.CurrentTeam != Team.Red)
        {
            // Skip Blue's turn
            if (game.Phase == GamePhase.AwaitingClue)
            {
                svc.SubmitClue(game, Team.Blue, "BLUEPASS", 0);
                svc.EndTurn(game, Team.Blue);
            }
        }
        
        svc.SubmitClue(game, Team.Red, "FINAL", 1);
        svc.MakeGuess(game, Team.Red, redPositions[8]); // Winning guess

        Assert.Equal(GamePhase.Complete, game.Phase);
        Assert.Equal(Team.Red, game.Winner);
        Assert.NotEmpty(game.TurnHistory);
        
        // Verify at least one turn shows game ended
        var gameEndingTurn = game.TurnHistory.FirstOrDefault(t => t.GameEnded);
        Assert.NotNull(gameEndingTurn);
        Assert.Equal(Team.Red, gameEndingTurn!.Winner);
    }

    [Fact]
    public void TurnHistory_MultipleTurnsAcrossBothTeams()
    {
        var svc = CreateService();
        var game = svc.CreateNewGame(Team.Red, seed: 7009);

        // Red turn 1
        svc.SubmitClue(game, Team.Red, "REDONE", 1);
        var redPos = game.Board.All.Select((c, i) => (c, i))
            .First(t => t.c.Type == CardType.RedAgent).i;
        svc.MakeGuess(game, Team.Red, redPos);
        svc.EndTurn(game, Team.Red);

        // Blue turn 1
        svc.SubmitClue(game, Team.Blue, "BLUEONE", 1);
        var bluePos = game.Board.All.Select((c, i) => (c, i))
            .First(t => t.c.Type == CardType.BlueAgent).i;
        svc.MakeGuess(game, Team.Blue, bluePos);
        svc.EndTurn(game, Team.Blue);

        // Red turn 2
        svc.SubmitClue(game, Team.Red, "REDTWO", 1);
        var neutralPos = game.Board.All.Select((c, i) => (c, i))
            .First(t => t.c.Type == CardType.Neutral).i;
        svc.MakeGuess(game, Team.Red, neutralPos); // Should end turn

        Assert.Equal(3, game.TurnHistory.Count);
        Assert.Equal(Team.Red, game.TurnHistory[0].Team);
        Assert.Equal(Team.Blue, game.TurnHistory[1].Team);
        Assert.Equal(Team.Red, game.TurnHistory[2].Team);
        
        Assert.True(game.TurnHistory[0].TurnEndedVoluntarily);
        Assert.True(game.TurnHistory[1].TurnEndedVoluntarily);
        Assert.False(game.TurnHistory[2].TurnEndedVoluntarily);
    }

    [Fact]
    public void TurnHistory_VerifyGuessEventFields()
    {
        var svc = CreateService();
        var game = svc.CreateNewGame(Team.Red, seed: 7010);
        svc.SubmitClue(game, Team.Red, "THETA", 2);

        var redPositions = game.Board.All.Select((c, i) => (c, i))
            .Where(t => t.c.Type == CardType.RedAgent)
            .Select(t => t.i)
            .Take(2)
            .ToList();

        svc.MakeGuess(game, Team.Red, redPositions[0]);
        svc.MakeGuess(game, Team.Red, redPositions[1]);

        Assert.NotNull(game.CurrentTurn);
        Assert.Equal(2, game.CurrentTurn!.Guesses.Count);
        
        var firstGuess = game.CurrentTurn.Guesses[0];
        var secondGuess = game.CurrentTurn.Guesses[1];
        
        // Verify guess positions are correct
        Assert.Equal(redPositions[0], firstGuess.Position);
        Assert.Equal(redPositions[1], secondGuess.Position);
        
        // Verify outcomes are correct
        Assert.Equal(GuessOutcome.FriendlyAgent, firstGuess.Outcome);
        Assert.Equal(GuessOutcome.FriendlyAgent, secondGuess.Outcome);
        
        // Verify card types are correct
        Assert.Equal(CardType.RedAgent, firstGuess.CardType);
        Assert.Equal(CardType.RedAgent, secondGuess.CardType);
        
        // Verify timestamps are reasonable (within last minute)
        var now = DateTime.UtcNow;
        Assert.True((now - firstGuess.Timestamp).TotalMinutes < 1);
        Assert.True((now - secondGuess.Timestamp).TotalMinutes < 1);
    }
}
