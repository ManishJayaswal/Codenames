namespace Codenames.Core.Domain.Words;

/// <summary>
/// Simple in-memory word provider with a static baseline list; shuffles deterministically when given a seeded RNG.
/// </summary>
public sealed class DefaultWordProvider : IWordProvider
{
    private static readonly string[] _baseWords = new[]
    {
        // A small starter list (>= 60 to allow future filtering if needed)
        "APPLE","MERCURY","SATELLITE","ORANGE","TABLE","PYRAMID","LONDON","STREAM","LASER","DRAGON",
        "NOVEL","GUITAR","MAMMOTH","ROBOT","PHOENIX","CANADA","NILE","GALAXY","SILK","EMERALD",
        "COMET","ANCHOR","MOUSE","KEY","WINDOW","SHADOW","FOREST","DESERT","OCEAN","MARBLE",
        "ENGINE","CLOUD","SPIRIT","CROWN","OPERA","JUPITER","PLUTO","VIOLET","QUARTZ","VECTOR",
        "CIRCUIT","ORBIT","BRIDGE","CRYSTAL","RAVEN","MONOLITH","BINARY","PLASMA","TIMBER","SPHINX",
        "SOCKET","STREAMLINE","ARCHIVE","FIBER","NEBULA","GLACIER","ISLAND","RUBY","SAPPHIRE","ONYX"
    };

    public IReadOnlyList<string> GetWords(int minimumCount, Random rng)
    {
        if (minimumCount < 0) throw new ArgumentOutOfRangeException(nameof(minimumCount));
        if (minimumCount > _baseWords.Length)
            throw new InvalidOperationException($"DefaultWordProvider does not have {minimumCount} unique words.");

        // Copy then Fisher-Yates shuffle using provided RNG for determinism.
        var arr = _baseWords.ToArray();
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
        return arr;
    }
}
