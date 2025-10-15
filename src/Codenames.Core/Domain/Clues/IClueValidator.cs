using Codenames.Core.Domain;

namespace Codenames.Core.Domain.Clues;

public interface IClueValidator
{
    /// <summary>
    /// Validates the proposed clue text for the current game state.
    /// Throws ClueValidationException if invalid.
    /// </summary>
    void Validate(Game game, string clueText, int declaredCount);
}
