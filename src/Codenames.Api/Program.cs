using Codenames.Api.Contracts;
using Codenames.Api.Infrastructure;
using Codenames.Core.Domain.Enums;
using Codenames.Core.Domain.Generation;
using Codenames.Core.Domain.Services;
using Codenames.Core.Domain.Words;
using Codenames.Core.Domain;
using Codenames.Core.Domain.Clues;
using Codenames.Core.Domain.Exceptions;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// JSON enum as strings
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Services
builder.Services.AddSingleton<IWordProvider, DefaultWordProvider>();
builder.Services.AddSingleton<BoardGenerator>();
builder.Services.AddSingleton<IClueValidator, BasicClueValidator>();
builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<IGameRepository, InMemoryGameRepository>();

var app = builder.Build();

// Static files for simple browser UI
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/games", (CreateGameRequest req, GameService gameService, IGameRepository repo) =>
{
    var starting = req.StartingTeam ?? Team.Red;
    var game = gameService.CreateNewGame(starting, req.Seed);
    repo.Add(game);
    // Return covered board by default so UI can display grouped counts
    return Results.Ok(new
    {
        game = game.ToCreateResponse(includeTypes: false),
        covered = game.ToCoveredBoard()
    });
});

app.MapGet("/games", (IGameRepository repo) =>
{
    var games = repo.All().Select(g => g.ToStateResponse(includeTypes: false));
    return Results.Ok(games);
});

// Spymaster view toggle via ?spymaster=true (shows all types unrevealed)
app.MapGet("/games/{id:guid}", (Guid id, bool? spymaster, IGameRepository repo) =>
{
    var game = repo.Get(id);
    if (game is null) return Results.NotFound();
    bool includeTypes = spymaster == true; // only when explicitly requested
    return Results.Ok(game.ToStateResponse(includeTypes));
});

app.MapGet("/games/{id:guid}/board/covered", (Guid id, IGameRepository repo) =>
{
    var game = repo.Get(id);
    return game is null ? Results.NotFound() : Results.Ok(game.ToCoveredBoard());
});

app.MapPost("/games/{id:guid}/clues", (Guid id, SubmitClueRequest req, IGameRepository repo, GameService service) =>
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
});

app.MapPost("/games/{id:guid}/guesses", (Guid id, GuessRequest req, IGameRepository repo, GameService service) =>
{
    var game = repo.Get(id);
    if (game is null) return Results.NotFound();
    try
    {
        var result = service.MakeGuess(game, req.Team, req.Position);
        repo.Update(game);
        return Results.Ok(new GuessResponse(game.Id, req.Position, result.Outcome.ToString(), result.TurnEnds, result.GameEnded, result.Winner, game.Phase,
            game.Board.All.Select(c => c.ToDto(includeType: c.IsRevealed)).ToList(), game.RedAgentsRemaining, game.BlueAgentsRemaining, game.CurrentClueGuesses, game.CurrentClue?.DeclaredCount + 1 ?? 0));
    }
    catch (DomainException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.Run();
