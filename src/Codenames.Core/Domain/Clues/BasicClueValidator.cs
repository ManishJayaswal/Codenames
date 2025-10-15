using System.Text.RegularExpressions;
using Codenames.Core.Domain.Exceptions;
using Codenames.Core.Domain.Enums;

namespace Codenames.Core.Domain.Clues;

/// <summary>
/// Implements Stage 4 basic clue rules:
/// - Single word letters only (A-Z)
/// - Not identical to any unrevealed board word (case-insensitive)
/// - Not a substring or superstring of any unrevealed board word (case-insensitive)
/// - Declared count >= 0
/// </summary>
public sealed class BasicClueValidator : IClueValidator
{
    private static readonly Regex LettersOnly = new("^[A-Za-z]+$", RegexOptions.Compiled);

    public void Validate(Game game, string clueText, int declaredCount)
    {
        if (game is null) throw new ArgumentNullException(nameof(game));
        if (string.IsNullOrWhiteSpace(clueText)) throw new ClueValidationException("Clue cannot be empty");
        if (declaredCount < 0) throw new ClueValidationException("Declared count cannot be negative");

        if (!LettersOnly.IsMatch(clueText))
            throw new ClueValidationException("Clue must be a single word with letters only");

        var upperClue = clueText.ToUpperInvariant();
        var unrevealedWords = game.Board.All
            .Where(c => !c.IsRevealed)
            .Select(c => c.Word)
            .Where(w => !string.IsNullOrEmpty(w))
            .Select(w => w.ToUpperInvariant())
            .ToList();

        // Identical
        if (unrevealedWords.Contains(upperClue))
            throw new ClueValidationException("Clue matches an unrevealed board word");

        // Substring or superstring
        foreach (var w in unrevealedWords)
        {
            if (w.Contains(upperClue) || upperClue.Contains(w))
                throw new ClueValidationException("Clue is a substring or superstring of an unrevealed word");
        }
    }
}
