using Codenames.Core.Domain.Enums;

namespace Codenames.Core.Domain;

/// <summary>
/// Immutable wrapper around the 5x5 board of game cards.
/// </summary>
public sealed class GameBoard
{
    public const int Rows = 5;
    public const int Columns = 5;
    public const int Size = Rows * Columns; // 25

    private readonly GameCard[] _cards; // fixed-size

    private GameBoard(GameCard[] cards)
    {
        _cards = cards;
    }

    public static GameBoard Create(IReadOnlyList<GameCard> cards)
    {
        if (cards is null) throw new ArgumentNullException(nameof(cards));
        if (cards.Count != Size)
            throw new ArgumentException($"Board must contain exactly {Size} cards", nameof(cards));

        // Validate positions align to index for early consistency (future generator responsible for correctness)
        for (int i = 0; i < Size; i++)
        {
            if (cards[i].Position != i)
                throw new ArgumentException("Card position must match its index", nameof(cards));
        }

        return new GameBoard(cards.ToArray());
    }

    /// <summary>
    /// Creates an empty placeholder board with blank words and neutral types (Stage 1 only).
    /// </summary>
    public static GameBoard CreateEmpty()
    {
        var cards = new GameCard[Size];
        for (int i = 0; i < Size; i++)
        {
            cards[i] = new GameCard(i, string.Empty, CardType.Neutral, false);
        }
        return new GameBoard(cards);
    }

    public GameCard this[int index]
    {
        get
        {
            if (index < 0 || index >= Size)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _cards[index];
        }
    }

    public IReadOnlyList<GameCard> All => _cards;
}
