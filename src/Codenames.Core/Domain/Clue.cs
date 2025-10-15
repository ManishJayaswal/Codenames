using Codenames.Core.Domain.Enums;

namespace Codenames.Core.Domain;

/// <summary>
/// A clue issued by a team's spymaster. Validation is intentionally minimal at Stage 1.
/// </summary>
public sealed record Clue
{
    public string Text { get; }
    public int DeclaredCount { get; }
    public Team Team { get; }
    public DateTime Timestamp { get; }

    public Clue(string text, int declaredCount, Team team, DateTime? timestamp = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Clue text cannot be empty", nameof(text));
        if (declaredCount < 0)
            throw new ArgumentOutOfRangeException(nameof(declaredCount), "Declared count cannot be negative");

        Text = text;
        DeclaredCount = declaredCount;
        Team = team;
        Timestamp = timestamp ?? DateTime.UtcNow;
    }
}
