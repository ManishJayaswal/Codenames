# Codenames (C#)

A web-based implementation of the Codenames board game built with .NET 10 and vanilla JavaScript.

## Features

- Full game logic: clue submission, guessing, turn management, win detection
- Browser UI with spymaster/operative views
- Deterministic board generation via optional seed
- In-memory game storage (multiple concurrent games supported)

## Quick Start

```bash
dotnet build
dotnet test
dotnet run --project src/Codenames.Api
```

Open `http://localhost:5000` in your browser to play.

## Projects

| Project | Description |
|---------|-------------|
| `Codenames.Core` | Domain model, game logic, services |
| `Codenames.Api` | ASP.NET Core Minimal API + static UI |
| `Codenames.Tests` | xUnit v3 test suite |

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/games` | Create a new game |
| GET | `/games` | List all games |
| GET | `/games/{id}` | Get game state (`?spymaster=true` to reveal card types) |
| GET | `/games/{id}/board/covered` | Get board with only revealed cards shown |
| POST | `/games/{id}/clues` | Submit a clue |
| POST | `/games/{id}/guesses` | Make a guess |
| POST | `/games/{id}/endturn` | End current turn |

## Game Rules

- 25 cards: 9 for starting team, 8 for other team, 7 neutral, 1 assassin
- Spymaster gives a one-word clue + number indicating how many cards match
- Operatives guess up to (clue count + 1) cards per turn
- Turn ends on: wrong guess, voluntary end, or guess limit reached
- Win: reveal all your team's agents. Lose: reveal the assassin.

## Documentation

See `docs/StageProgress.md` for development history.
