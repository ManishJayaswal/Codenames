using System.Collections.Concurrent;
using Codenames.Core.Domain;

namespace Codenames.Api.Infrastructure;

public sealed class InMemoryGameRepository : IGameRepository
{
    private readonly ConcurrentDictionary<Guid, Game> _store = new();

    public Game Add(Game game)
    {
        if (!_store.TryAdd(game.Id, game))
            throw new InvalidOperationException("Game with same id already exists");
        return game;
    }

    public Game? Get(Guid id) => _store.TryGetValue(id, out var g) ? g : null;

    public IEnumerable<Game> All() => _store.Values;

    public void Update(Game game)
    {
        _store[game.Id] = game; // aggregate mutates internally; replace reference for safety
    }
}
