using Codenames.Core.Domain.Enums;

namespace Codenames.Api.Contracts.Requests;

public sealed record CreateGameRequest(Team? StartingTeam, int? Seed);
public sealed record SubmitClueRequest(string Clue, int Count, Team Team);
public sealed record GuessRequest(Team Team, int Position);
public sealed record EndTurnRequest(Team Team);
