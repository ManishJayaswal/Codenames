namespace Codenames.Core.Domain.Words;

/// <summary>
/// Supplies a sequence of candidate words for board generation.
/// Implementation must ensure at least 25 unique words when requested.
/// </summary>
public interface IWordProvider
{
    /// <summary>
    /// Returns a collection of unique words (case-insensitive uniqueness) with at least the requested count.
    /// Excess words may be returned; caller will take the needed subset.
    /// </summary>
    IReadOnlyList<string> GetWords(int minimumCount, Random rng);
}
