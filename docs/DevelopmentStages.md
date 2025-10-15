# Development Stages Plan

(S) Stage, (D) Deliverables, (T) Tests, (A) Acceptance.

Stage 0  Repo & Spec
D: Solution, projects, docs, CI.
T: Build/test pipeline.
A: Clean build.

Stage 1  Domain Skeleton
D: Enums + GameBoard + GameCard + Game + Clue.
T: Board size, index access, invalid positions.
A: Tests pass.

Stage 2 – Word Provider & Board Generation
D: IWordProvider, BoardGenerator.
T: Distribution counts, uniqueness, deterministic seed.

Stage 3  Game Creation
D: GameService.CreateNewGame.
T: Counts & starting team selection.

Stage 4 – Clue Submission
D: SubmitClue + basic validator.
T: Valid/invalid clue paths.

Stage 5  Guess Outcomes
D: RevealGuess logic.
T: Assassin, neutral, wrong-team, friendly.

Stage 6  Multi-Guess & Limits
D: Count+1 logic, EndTurn.
T: Sequence limit enforcement.

Stage 7  Win Conditions & History
D: TurnRecord + win detection.
T: Immediate win states + history.

Stage 8  API Read Only
D: Create/get/list endpoints.
T: Hide/unhide unrevealed types.

Stage 9  API Mutations
D: Clue/guess/endturn endpoints.

Stage 10  Front-End Shell
D: Static scaffolding.

Stage 11  Board Rendering
D: Render grid, spymaster toggle.

Stage 12 – Clue UI
D: Clue submission components.

Stage 13 – Guess Interaction
D: Click-to-guess integration.

Stage 14  History Panel
D: Display turn history.

Stage 15  Validator Hardening
D: Plural/substring improvements.

Stage 16  Stabilization
D: Coverage 70%, refactors.

Stage 17 – Enhancements
Menu-based optional features.
