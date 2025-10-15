using Codenames.Core.Domain;
using Codenames.Core.Domain.Enums;

namespace Codenames.Api.Contracts;

public sealed record CreateGameRequest(Team? StartingTeam, int? Seed);
public sealed record CreateGameResponse(Guid GameId, Team StartingTeam, Team CurrentTeam, GamePhase Phase, int RedAgentsRemaining, int BlueAgentsRemaining, IReadOnlyList<GameCardDto> Board, Team? Winner);
public sealed record GameCardDto(int Position, string Word, string? Type, bool Revealed);
public sealed record SubmitClueRequest(string Clue, int Count, Team Team);
public sealed record SubmitClueResponse(Guid GameId, string Clue, int Count, Team Team, GamePhase Phase);
public sealed record GuessRequest(Team Team, int Position);
public sealed record GuessResponse(Guid GameId, int Position, string Outcome, bool TurnEnded, bool GameEnded, Team? Winner, GamePhase Phase, IReadOnlyList<GameCardDto> Board, int RedAgentsRemaining, int BlueAgentsRemaining, int CurrentClueGuesses, int MaxGuesses);
public sealed record GameStateResponse(Guid GameId, Team StartingTeam, Team CurrentTeam, GamePhase Phase, int RedAgentsRemaining, int BlueAgentsRemaining, Team? Winner, IReadOnlyList<GameCardDto> Board, IReadOnlyList<ClueDto> Clues, int? CurrentClueGuesses, int? CurrentClueMaxGuesses);
public sealed record ClueDto(string Text, int DeclaredCount, Team Team, DateTime Timestamp);

// Covered / grouped board view (words hidden unless revealed) grouped by team/color category
public sealed record CoveredBoardResponse(Guid GameId, IReadOnlyList<CoveredCardGroup> Groups);
public sealed record CoveredCardGroup(string Category, IReadOnlyList<CoveredCardDto> Cards);
public sealed record CoveredCardDto(int Position, string? Word, bool Revealed);

public static class GameMappings
{
    public static GameCardDto ToDto(this GameCard card, bool includeType)
        => new(card.Position, card.Word, includeType ? card.Type.ToString() : null, card.IsRevealed);

    public static CreateGameResponse ToCreateResponse(this Game game, bool includeTypes)
        => new(game.Id, game.StartingTeam, game.CurrentTeam, game.Phase, game.RedAgentsRemaining, game.BlueAgentsRemaining,
            game.Board.All.Select(c => c.ToDto(includeTypes || c.IsRevealed)).ToList(), game.Winner);

    public static GameStateResponse ToStateResponse(this Game game, bool includeTypes)
        => new(game.Id, game.StartingTeam, game.CurrentTeam, game.Phase, game.RedAgentsRemaining, game.BlueAgentsRemaining, game.Winner,
            game.Board.All.Select(c => c.ToDto(includeTypes || c.IsRevealed)).ToList(),
            game.ClueHistory.Select(c => new ClueDto(c.Text, c.DeclaredCount, c.Team, c.Timestamp)).ToList(),
            game.CurrentClue is null ? null : game.CurrentClueGuesses,
            game.CurrentClue is null ? null : game.CurrentClue.DeclaredCount + 1);

    public static CoveredBoardResponse ToCoveredBoard(this Game game)
    {
        var groups = game.Board.All
            .GroupBy(c => c.Type)
            .OrderBy(g => g.Key switch
            {
                CardType.RedAgent => 0,
                CardType.BlueAgent => 1,
                CardType.Neutral => 2,
                CardType.Assassin => 3,
                _ => 4
            })
            .Select(g => new CoveredCardGroup(
                Category: g.Key switch
                {
                    CardType.RedAgent => "Red",
                    CardType.BlueAgent => "Blue",
                    CardType.Neutral => "Neutral",
                    CardType.Assassin => "Assassin",
                    _ => g.Key.ToString()
                },
                Cards: g.Select(c => new CoveredCardDto(c.Position, c.IsRevealed ? c.Word : null, c.IsRevealed)).ToList()
            ))
            .ToList();
        return new CoveredBoardResponse(game.Id, groups);
    }
}
