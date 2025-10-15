using Codenames.Core.Domain;

namespace Codenames.Api.Infrastructure;

public interface IGameRepository
{
    Game Add(Game game);
    Game? Get(Guid id);
    void Update(Game game);
    IEnumerable<Game> All();
}
