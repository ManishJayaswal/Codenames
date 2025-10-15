using Codenames.Core.Domain.Services;
using Codenames.Core.Domain.Generation;
using Codenames.Core.Domain.Words;
using Codenames.Core.Domain.Enums;
using Codenames.Core.Domain.Exceptions;

namespace Codenames.Tests.Domain;

public class ClueSubmissionTests
{
    private GameService CreateService(int seed) => new GameService(new BoardGenerator(new DefaultWordProvider()));

    [Fact]
    public void SubmitClue_Valid_TransitionsPhase()
    {
        var svc = CreateService(100);
        var game = svc.CreateNewGame(Team.Red, seed: 100);

        svc.SubmitClue(game, Team.Red, "ALPHA", 2);

        Assert.Equal(GamePhase.AwaitingGuesses, game.Phase);
        Assert.NotNull(game.CurrentClue);
        Assert.Equal("ALPHA", game.CurrentClue!.Text);
        Assert.Single(game.ClueHistory);
    }

    [Fact]
    public void SubmitClue_WrongPhase_Throws()
    {
        var svc = CreateService(100);
        var game = svc.CreateNewGame(Team.Red, seed: 100);
        // First submit moves to AwaitingGuesses
        svc.SubmitClue(game, Team.Red, "BRAVO", 1);
        Assert.Throws<InvalidPhaseException>(() => svc.SubmitClue(game, Team.Red, "CHARLIE", 1));
    }

    [Fact]
    public void SubmitClue_TeamMismatch_Throws()
    {
        var svc = CreateService(100);
        var game = svc.CreateNewGame(Team.Red, seed: 100);
        Assert.Throws<TeamMismatchException>(() => svc.SubmitClue(game, Team.Blue, "DELTA", 1));
    }

    [Theory]
    [InlineData("", 1, "Clue cannot be empty")]
    [InlineData("ALPHA BETA", 1, "single word")]
    [InlineData("ALPHA1", 1, "letters only")]
    public void SubmitClue_InvalidFormat_Throws(string clue, int count, string expectedFragment)
    {
        var svc = CreateService(100);
        var game = svc.CreateNewGame(Team.Red, seed: 100);
        var ex = Assert.Throws<ClueValidationException>(() => svc.SubmitClue(game, Team.Red, clue, count));
        Assert.Contains("Invalid clue", ex.Message);
        Assert.Contains(expectedFragment, ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SubmitClue_IdenticalToBoardWord_Throws()
    {
        var svc = CreateService(100);
        var game = svc.CreateNewGame(Team.Red, seed: 100);
        // Use an unrevealed board word
        var word = game.Board[0].Word; // board words are uppercase; validator uppercases
        var ex = Assert.Throws<ClueValidationException>(() => svc.SubmitClue(game, Team.Red, word, 1));
        Assert.Contains("matches an unrevealed board word", ex.Message);
    }

    [Fact]
    public void SubmitClue_SubstringRelation_Throws()
    {
        var svc = CreateService(100);
        var game = svc.CreateNewGame(Team.Red, seed: 100);
        var word = game.Board[0].Word; // e.g., "APPLE"; try APP
        var substr = word.Substring(0, Math.Min(3, word.Length));
        var ex = Assert.Throws<ClueValidationException>(() => svc.SubmitClue(game, Team.Red, substr, 1));
        Assert.Contains("substring or superstring", ex.Message);
    }
}
