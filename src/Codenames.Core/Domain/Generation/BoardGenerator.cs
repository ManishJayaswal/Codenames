using Codenames.Core.Domain.Enums;
using Codenames.Core.Domain.Words;

namespace Codenames.Core.Domain.Generation;

/// <summary>
/// Responsible for producing a populated <see cref="GameBoard"/> given a word provider and optional seed.
/// Distribution rules (Stage 2):
/// - Starting team gets 9 agents, other team 8
/// - 7 Neutral
/// - 1 Assassin
/// </summary>
public sealed class BoardGenerator
{
    private readonly IWordProvider _wordProvider;

    public BoardGenerator(IWordProvider wordProvider)
    {
        _wordProvider = wordProvider ?? throw new ArgumentNullException(nameof(wordProvider));
    }

    private const int RedCount = 9;
    private const int BlueCount = 8;
    private const int NeutralCount = 7;
    private const int AssassinCount = 1;

    public GameBoard Generate(Team startingTeam, int? seed = null)
    {
        var rng = seed.HasValue ? new Random(seed.Value) : new Random();

        var words = _wordProvider.GetWords(GameBoard.Size, rng);

        // assign types list with distribution then shuffle positions
        var redAgents = startingTeam == Team.Red ? RedCount : BlueCount;
        var blueAgents = startingTeam == Team.Red ? BlueCount : RedCount; // inverse if Blue starts

        var types = new List<CardType>(GameBoard.Size);
        types.AddRange(Enumerable.Repeat(startingTeam == Team.Red ? CardType.RedAgent : CardType.BlueAgent, redAgents));
        types.AddRange(Enumerable.Repeat(startingTeam == Team.Red ? CardType.BlueAgent : CardType.RedAgent, blueAgents));
        types.AddRange(Enumerable.Repeat(CardType.Neutral, NeutralCount));
        types.Add(CardType.Assassin);

        if (types.Count != GameBoard.Size)
            throw new InvalidOperationException("Distribution counts do not sum to board size.");

        // Shuffle types deterministically with same RNG instance to tie to seed
        for (int i = types.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (types[i], types[j]) = (types[j], types[i]);
        }

        var cards = new GameCard[GameBoard.Size];
        for (int i = 0; i < GameBoard.Size; i++)
        {
            cards[i] = new GameCard(i, words[i], types[i]);
        }
        return GameBoard.Create(cards);
    }
}
