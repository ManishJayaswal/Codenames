using Codenames.Core.Domain.Enums;
using Codenames.Core.Domain.Generation;
using Codenames.Core.Domain.Clues;
using Codenames.Core.Domain.Exceptions;
using Codenames.Core.Domain.Guesses;

namespace Codenames.Core.Domain.Services;

/// <summary>
/// Responsible for creating new games (Stage 3). Later stages will extend with clue submission & guess handling.
/// </summary>
public sealed class GameService
{
    private readonly BoardGenerator _boardGenerator;
    private readonly IClueValidator _clueValidator;

    public GameService(BoardGenerator boardGenerator, IClueValidator? clueValidator = null)
    {
        _boardGenerator = boardGenerator ?? throw new ArgumentNullException(nameof(boardGenerator));
        _clueValidator = clueValidator ?? new BasicClueValidator();
    }

    /// <summary>
    /// Creates a new game using the internal board generator. Applies distribution counts to remaining agents.
    /// </summary>
    /// <param name="startingTeam">The team that will start (gets 9 agents).</param>
    /// <param name="seed">Optional seed for deterministic board generation.</param>
    public Game CreateNewGame(Team startingTeam, int? seed = null, Guid? gameId = null)
    {
        var board = _boardGenerator.Generate(startingTeam, seed);
        // Count agents based on card types.
        int redAgents = board.All.Count(c => c.Type == CardType.RedAgent);
        int blueAgents = board.All.Count(c => c.Type == CardType.BlueAgent);

        // Starting state enters AwaitingClue (first action will be clue submission).
        var game = Game.CreateFromGeneratedBoard(board, startingTeam, redAgents, blueAgents, gameId);
        return game;
    }

    /// <summary>
    /// Submits a clue for the current team. Validates clue and transitions phase.
    /// </summary>
    public void SubmitClue(Game game, Team team, string clueText, int declaredCount)
    {
        if (game is null) throw new ArgumentNullException(nameof(game));
        if (game.Phase != GamePhase.AwaitingClue)
            throw new InvalidPhaseException("SubmitClue", GamePhase.AwaitingClue.ToString(), game.Phase.ToString());
        if (team != game.CurrentTeam)
            throw new TeamMismatchException($"Team {team} cannot submit clue; current team is {game.CurrentTeam}.");

        _clueValidator.Validate(game, clueText, declaredCount);
        var clue = new Clue(clueText, declaredCount, team);
        game.AddClue(clue);
    }

    /// <summary>
    /// Makes a guess at the specified position. Returns result info including whether turn/game ended.
    /// Enforces max guesses: declaredCount + 1 (extra guess rule) for the current clue.
    /// If the limit is reached after a correct guess that does not end the game, the turn ends.
    /// </summary>
    public GuessResult MakeGuess(Game game, Team team, int position)
    {
        if (game is null) throw new ArgumentNullException(nameof(game));
        if (game.Phase != GamePhase.AwaitingGuesses)
            throw new InvalidPhaseException("MakeGuess", GamePhase.AwaitingGuesses.ToString(), game.Phase.ToString());
        if (team != game.CurrentTeam)
            throw new TeamMismatchException($"Team {team} cannot guess; current team is {game.CurrentTeam}.");
        if (game.CurrentClue is null)
            throw new InvalidOperationException("No active clue.");

        // Enforce not exceeding maximum allowed guesses before making the guess.
        int maxGuesses = game.CurrentClue.DeclaredCount + 1; // extra guess rule
        if (game.CurrentClueGuesses >= maxGuesses)
            throw new InvalidGuessException("Maximum guesses for this clue already taken.");

        var (cardType, turnEnds, gameEnded, winner) = game.RevealGuess(position, team);

        GuessOutcome outcome = cardType switch
        {
            Enums.CardType.Assassin => GuessOutcome.Assassin,
            Enums.CardType.Neutral => GuessOutcome.Neutral,
            Enums.CardType.RedAgent => team == Team.Red ? GuessOutcome.FriendlyAgent : GuessOutcome.OpponentAgent,
            Enums.CardType.BlueAgent => team == Team.Blue ? GuessOutcome.FriendlyAgent : GuessOutcome.OpponentAgent,
            _ => throw new InvalidOperationException("Unexpected card type")
        };

        // Increment guess counter only if the card revealed was an agent for current team (a valid continuing guess) OR opponent agent? Actually any successful reveal attempt counts toward guesses taken regardless of result, except assassin? Convention: count all revealed attempts.
        game.IncrementGuessCounter();

        // Apply guess limit logic if still in guessing phase and not ended by card resolution.
        if (!turnEnds && !gameEnded)
        {
            if (game.CurrentClueGuesses >= maxGuesses)
            {
                // Force turn end due to reaching guess cap.
                turnEnds = true;
                game.Phase = GamePhase.AwaitingClue;
                game.CurrentTeam = team == Team.Red ? Team.Blue : Team.Red;
            }
        }

        return new GuessResult(position, outcome, team, turnEnds, gameEnded, winner);
    }
}
