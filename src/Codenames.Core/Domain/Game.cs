using Codenames.Core.Domain.Enums;
using Codenames.Core.Domain.Turns;

namespace Codenames.Core.Domain;

/// <summary>
/// Aggregate root for the game. Stage 1 keeps it minimal; richer lifecycle appears in later stages.
/// </summary>
public sealed class Game
{
    public Guid Id { get; }
    public GamePhase Phase { get; internal set; }
    public Team StartingTeam { get; }
    public Team CurrentTeam { get; internal set; }
    public GameBoard Board { get; internal set; }
    public int RedAgentsRemaining { get; internal set; }
    public int BlueAgentsRemaining { get; internal set; }
    public Team? Winner { get; internal set; }

    private readonly List<Clue> _clueHistory = new();
    public IReadOnlyList<Clue> ClueHistory => _clueHistory;
    public Clue? CurrentClue => _clueHistory.LastOrDefault();

    // Stage 7: Turn history tracking
    private readonly List<TurnRecord> _turnHistory = new();
    public IReadOnlyList<TurnRecord> TurnHistory => _turnHistory;
    public CurrentTurn? CurrentTurn { get; private set; }

    // Number of guesses taken under the current (most recent) clue.
    // Migrated to use CurrentTurn.GuessCount when available (Stage 7).
    public int CurrentClueGuesses => CurrentTurn?.GuessCount ?? 0;

    private Game(Guid id, Team startingTeam, GameBoard board)
    {
        Id = id;
        StartingTeam = startingTeam;
        CurrentTeam = startingTeam;
        Phase = GamePhase.Setup; // will transition in later stages when board is populated / game starts
        Board = board;
        RedAgentsRemaining = 0; // assigned in Stage 3 when generation logic is known
        BlueAgentsRemaining = 0;
    }

    public static Game CreateNew(Guid? id = null, Team startingTeam = Team.Red)
    {
        var board = GameBoard.CreateEmpty();
        return new Game(id ?? Guid.NewGuid(), startingTeam, board);
    }

    /// <summary>
    /// Internal factory used by GameService once a board has been generated with proper distribution.
    /// Sets phase to AwaitingClue because the game is ready for the first clue.
    /// </summary>
    internal static Game CreateFromGeneratedBoard(GameBoard board, Team startingTeam, int redAgentsRemaining, int blueAgentsRemaining, Guid? id = null)
    {
        var game = new Game(id ?? Guid.NewGuid(), startingTeam, board)
        {
            RedAgentsRemaining = redAgentsRemaining,
            BlueAgentsRemaining = blueAgentsRemaining,
            Phase = GamePhase.AwaitingClue
        };
        return game;
    }

    /// <summary>
    /// Adds a validated clue to the game and transitions phase to AwaitingGuesses.
    /// Creates a new CurrentTurn to track guesses for this clue (Stage 7).
    /// </summary>
    internal void AddClue(Clue clue)
    {
        _clueHistory.Add(clue);
        CurrentTurn = new CurrentTurn(CurrentTeam, clue);
        Phase = GamePhase.AwaitingGuesses;
    }

    /// <summary>
    /// Records a guess in the current turn (Stage 7).
    /// </summary>
    internal void RecordGuess(GuessEvent guessEvent)
    {
        CurrentTurn?.AddGuess(guessEvent);
    }

    /// <summary>
    /// Finalizes the current turn and adds it to history (Stage 7).
    /// </summary>
    internal void FinalizeTurn(bool voluntaryEnd, bool gameEnded, Team? winner)
    {
        if (CurrentTurn is not null)
        {
            _turnHistory.Add(CurrentTurn.ToRecord(voluntaryEnd, gameEnded, winner));
            CurrentTurn = null;
        }
    }

    /// <summary>
    /// Reveals the card at the given index and applies outcome side-effects.
    /// Returns tuple: (cardType, turnEnds, gameEnded, winner, outcomeCategoryString)
    /// Actual interpretation into GuessOutcome occurs in service layer to keep enum dependency localized.
    /// </summary>
    internal (Enums.CardType cardType, bool turnEnds, bool gameEnded, Team? winner) RevealGuess(int index, Team guessingTeam)
    {
        if (index < 0 || index >= GameBoard.Size) throw new ArgumentOutOfRangeException(nameof(index));
        var card = Board[index];
        if (card.IsRevealed) throw new Exceptions.InvalidGuessException("Card already revealed");

        // Mutate board by replacing card with revealed version.
        var cards = Board.All.ToArray();
        cards[index] = card.Reveal();
        Board = GameBoard.Create(cards); // simple immutable replace

        bool turnEnds = false;
        bool gameEnded = false;
        Team? winner = null;

        switch (card.Type)
        {
            case Enums.CardType.RedAgent:
                if (guessingTeam == Team.Red)
                {
                    RedAgentsRemaining--;
                    if (RedAgentsRemaining == 0)
                    {
                        gameEnded = true; winner = Team.Red; Winner = winner; Phase = GamePhase.Complete;
                    }
                }
                else
                {
                    // opponent agent -> turn ends, decrement their remaining
                    RedAgentsRemaining--;
                    turnEnds = true;
                    CurrentTeam = Team.Red; // control passes to that team next turn
                }
                break;
            case Enums.CardType.BlueAgent:
                if (guessingTeam == Team.Blue)
                {
                    BlueAgentsRemaining--;
                    if (BlueAgentsRemaining == 0)
                    {
                        gameEnded = true; winner = Team.Blue; Winner = winner; Phase = GamePhase.Complete;
                    }
                }
                else
                {
                    BlueAgentsRemaining--;
                    turnEnds = true;
                    CurrentTeam = Team.Blue;
                }
                break;
            case Enums.CardType.Neutral:
                turnEnds = true;
                CurrentTeam = guessingTeam == Team.Red ? Team.Blue : Team.Red;
                break;
            case Enums.CardType.Assassin:
                // Assassin: immediate win for the opposing team.
                gameEnded = true;
                winner = guessingTeam == Team.Red ? Team.Blue : Team.Red;
                Winner = winner;
                Phase = GamePhase.Complete;
                turnEnds = true;
                break;
        }

        // If turn ended but game not complete, transition to AwaitingClue and no winner.
        if (turnEnds && !gameEnded)
        {
            Phase = GamePhase.AwaitingClue;
        }
        else if (!turnEnds && !gameEnded)
        {
            // Still guessing on same clue.
            Phase = GamePhase.AwaitingGuesses;
        }

        return (card.Type, turnEnds, gameEnded, winner);
    }
}
