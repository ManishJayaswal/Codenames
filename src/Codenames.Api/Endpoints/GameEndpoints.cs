using Codenames.Api.Contracts.Mappings;
using Codenames.Api.Contracts.Requests;
using Codenames.Api.Contracts.Responses;
using Codenames.Core.Domain.Abstractions;
using Codenames.Core.Domain.Enums;
using Codenames.Core.Domain.Exceptions;
using Codenames.Core.Domain.Services;

namespace Codenames.Api.Endpoints;

/// <summary>
/// Maps all game-related HTTP endpoints.
/// </summary>
public static class GameEndpoints
{
    public static void MapGameEndpoints(this WebApplication app)
    {
        app.MapPost("/games", CreateGame);
        app.MapGet("/games", ListGames);
        app.MapGet("/games/{id:guid}", GetGame);
        app.MapGet("/games/{id:guid}/board/covered", GetCoveredBoard);
        app.MapPost("/games/{id:guid}/clues", SubmitClue);
        app.MapPost("/games/{id:guid}/guesses", MakeGuess);
        app.MapPost("/games/{id:guid}/endturn", EndTurn);
    }

    private static IResult CreateGame(CreateGameRequest req, GameService gameService, IGameRepository repo)
    {
        var starting = req.StartingTeam ?? Team.Red;
        var game = gameService.CreateNewGame(starting, req.Seed);
        repo.Add(game);
        return Results.Ok(new
        {
            game = game.ToCreateResponse(includeTypes: false),
            covered = game.ToCoveredBoard()
        });
    }

    private static IResult ListGames(IGameRepository repo)
    {
        var games = repo.All().Select(g => g.ToStateResponse(includeTypes: false));
        return Results.Ok(games);
    }

    private static IResult GetGame(Guid id, bool? spymaster, IGameRepository repo)
    {
        var game = repo.Get(id);
        if (game is null) return Results.NotFound();
        bool includeTypes = spymaster == true;
        return Results.Ok(game.ToStateResponse(includeTypes));
    }

    private static IResult GetCoveredBoard(Guid id, IGameRepository repo)
    {
        var game = repo.Get(id);
        return game is null ? Results.NotFound() : Results.Ok(game.ToCoveredBoard());
    }

    private static IResult SubmitClue(Guid id, SubmitClueRequest req, IGameRepository repo, GameService service)
    {
        var game = repo.Get(id);
        if (game is null) return Results.NotFound();
        try
        {
            service.SubmitClue(game, req.Team, req.Clue, req.Count);
            repo.Update(game);
            return Results.Ok(new SubmitClueResponse(game.Id, req.Clue, req.Count, req.Team, game.Phase));
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static IResult MakeGuess(Guid id, GuessRequest req, IGameRepository repo, GameService service)
    {
        var game = repo.Get(id);
        if (game is null) return Results.NotFound();
        try
        {
            var result = service.MakeGuess(game, req.Team, req.Position);
            repo.Update(game);
            return Results.Ok(new GuessResponse(
                game.Id,
                req.Position,
                result.Outcome.ToString(),
                result.TurnEnds,
                result.GameEnded,
                result.Winner,
                game.Phase,
                game.Board.All.Select(c => c.ToDto(includeType: c.IsRevealed)).ToList(),
                game.RedAgentsRemaining,
                game.BlueAgentsRemaining,
                game.CurrentClueGuesses,
                game.CurrentClue?.DeclaredCount + 1 ?? 0));
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static IResult EndTurn(Guid id, EndTurnRequest req, IGameRepository repo, GameService service)
    {
        var game = repo.Get(id);
        if (game is null) return Results.NotFound();
        try
        {
            service.EndTurn(game, req.Team);
            repo.Update(game);
            return Results.Ok(game.ToStateResponse(includeTypes: false));
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
