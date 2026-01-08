using Codenames.Api.Contracts.Responses;
using Codenames.Core.Domain;
using Codenames.Core.Domain.Enums;

namespace Codenames.Api.Contracts.Mappings;

/// <summary>
/// Extension methods for mapping domain objects to API response DTOs.
/// </summary>
public static class GameMappings
{
    public static GameCardDto ToDto(this GameCard card, bool includeType)
        => new(card.Position, card.Word, includeType ? card.Type.ToString() : null, card.IsRevealed);

    public static CreateGameResponse ToCreateResponse(this Game game, bool includeTypes)
        => new(
            game.Id,
            game.StartingTeam,
            game.CurrentTeam,
            game.Phase,
            game.RedAgentsRemaining,
            game.BlueAgentsRemaining,
            game.Board.All.Select(c => c.ToDto(includeTypes || c.IsRevealed)).ToList(),
            game.Winner);

    public static GameStateResponse ToStateResponse(this Game game, bool includeTypes)
        => new(
            game.Id,
            game.StartingTeam,
            game.CurrentTeam,
            game.Phase,
            game.RedAgentsRemaining,
            game.BlueAgentsRemaining,
            game.Winner,
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
