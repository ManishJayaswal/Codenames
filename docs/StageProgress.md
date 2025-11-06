# Stage Progress

## Completed
- Stage 0: Repository, solution skeleton, specs.
- Stage 1: Domain skeleton (enums, GameCard, GameBoard, Game, Clue) + foundational tests.
- Stage 2: Word Provider & Board Generation (deterministic seed, distribution, uniqueness tests).
- Stage 3: GameService.CreateNewGame (board generation integration, counts, phase transition to AwaitingClue).
- Stage 4: Clue submission (validator, phase transition to AwaitingGuesses, validation exceptions, tests for invalid forms & team/phase errors).
- Stage 5: Guess outcomes (reveal logic, outcome classification, team switching, win detection on assassin / all agents, tests for all outcome paths).
- Stage 6: Multi-guess & limits (Count+1, EndTurn mechanics, guess limit enforcement).
- Stage 7: Win conditions & history (TurnRecord, GuessEvent, CurrentTurn, turn history tracking, tests for all end scenarios and history correctness).

## In Progress
None.

## Next Planned
- Stage 8: API Read Only (Create/get/list endpoints, hide/unhide unrevealed types).

This file will expand as stages complete to give a concise status snapshot.
