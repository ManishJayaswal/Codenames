using Codenames.Core.Domain.Enums;

namespace Codenames.Api.Contracts.Responses;

public sealed record CreateGameResponse(
    Guid GameId,
    Team StartingTeam,
    Team CurrentTeam,
    GamePhase Phase,
    int RedAgentsRemaining,
    int BlueAgentsRemaining,
    IReadOnlyList<GameCardDto> Board,
    Team? Winner);

public sealed record GameStateResponse(
    Guid GameId,
    Team StartingTeam,
    Team CurrentTeam,
    GamePhase Phase,
    int RedAgentsRemaining,
    int BlueAgentsRemaining,
    Team? Winner,
    IReadOnlyList<GameCardDto> Board,
    IReadOnlyList<ClueDto> Clues,
    int? CurrentClueGuesses,
    int? CurrentClueMaxGuesses);

public sealed record SubmitClueResponse(Guid GameId, string Clue, int Count, Team Team, GamePhase Phase);

public sealed record GuessResponse(
    Guid GameId,
    int Position,
    string Outcome,
    bool TurnEnded,
    bool GameEnded,
    Team? Winner,
    GamePhase Phase,
    IReadOnlyList<GameCardDto> Board,
    int RedAgentsRemaining,
    int BlueAgentsRemaining,
    int CurrentClueGuesses,
    int MaxGuesses);

public sealed record CoveredBoardResponse(Guid GameId, IReadOnlyList<CoveredCardGroup> Groups);
public sealed record CoveredCardGroup(string Category, IReadOnlyList<CoveredCardDto> Cards);

// Shared DTOs used in responses
public sealed record GameCardDto(int Position, string Word, string? Type, bool Revealed);
public sealed record ClueDto(string Text, int DeclaredCount, Team Team, DateTime Timestamp);
public sealed record CoveredCardDto(int Position, string? Word, bool Revealed);
