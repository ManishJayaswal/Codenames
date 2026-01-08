using Codenames.Api.Endpoints;
using Codenames.Api.Infrastructure;
using Codenames.Core.Domain.Abstractions;
using Codenames.Core.Domain.Clues;
using Codenames.Core.Domain.Generation;
using Codenames.Core.Domain.Services;
using Codenames.Core.Domain.Words;
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

// Map all game endpoints
app.MapGameEndpoints();

app.Run();
