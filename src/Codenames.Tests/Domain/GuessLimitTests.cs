using Codenames.Core.Domain.Enums;
using Codenames.Core.Domain.Generation;
using Codenames.Core.Domain.Services;
using Codenames.Core.Domain.Words;
using Codenames.Core.Domain.Exceptions;

namespace Codenames.Tests.Domain;

public class GuessLimitTests
{
    private GameService CreateService() => new GameService(new BoardGenerator(new DefaultWordProvider()));

    [Fact]
    public void GuessLimit_DeclaredCountPlusOne_Enforced()
    {
        var svc = CreateService();
        var game = svc.CreateNewGame(Team.Red, seed: 3000);
        svc.SubmitClue(game, Team.Red, "OMEGA", 1); // max guesses allowed = 2

        // Collect friendly agent positions for deterministic test (ensure at least two exist)
        var redPositions = game.Board.All.Select((c,i)=>(c,i)).Where(t=>t.c.Type==CardType.RedAgent).Select(t=>t.i).ToList();
        Assert.True(redPositions.Count > 1);

        // First guess friendly
        var r1 = svc.MakeGuess(game, Team.Red, redPositions[0]);
        Assert.False(r1.TurnEnds); // still can guess second time
        Assert.Equal(GamePhase.AwaitingGuesses, game.Phase);

        // Second guess friendly should now end turn due to limit reached (if not game-winning). Pick another red.
        if (game.Phase != GamePhase.Complete)
        {
            var r2 = svc.MakeGuess(game, Team.Red, redPositions[1]);
            // Turn should end because of cap unless victory triggered.
            if (game.Phase != GamePhase.Complete)
            {
                Assert.True(r2.TurnEnds);
                Assert.Equal(GamePhase.AwaitingClue, game.Phase);
                Assert.Equal(Team.Blue, game.CurrentTeam);
            }
        }
    }

    [Fact]
    public void GuessLimit_Exceeded_Throws()
    {
        var svc = CreateService();
        var game = svc.CreateNewGame(Team.Red, seed: 3001);
        svc.SubmitClue(game, Team.Red, "SIGMA", 0); // max guesses = 1

        // Make one guess (any unrevealed position)
        int pos = game.Board.All.Select((c,i)=>(c,i)).First().i;
        _ = svc.MakeGuess(game, Team.Red, pos);

        if (game.Phase == GamePhase.AwaitingGuesses) // if turn not ended (friendly agent and not win), should now be capped
        {
            Assert.Throws<InvalidGuessException>(() => svc.MakeGuess(game, Team.Red, pos == 0 ? 1 : 0));
        }
    }
}
