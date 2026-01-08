namespace Codenames.Core.Domain.Abstractions;

/// <summary>
/// Repository abstraction for persisting and retrieving games.
/// </summary>
public interface IGameRepository
{
    Game Add(Game game);
    Game? Get(Guid id);
    void Update(Game game);
    IEnumerable<Game> All();
}
