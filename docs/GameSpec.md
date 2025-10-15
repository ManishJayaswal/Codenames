# Codenames  Core Game Specification (Baseline for Implementation)

Version: 1.0 (Covers Stages 07 scope; API/UI sections evolve later)

## 1. Objective
Two teams (Red, Blue) race to identify all their agents via one‑word clues, avoiding the assassin.

## 2. Board
- Size: 5×5 (25 unique word cards)
- Card Distribution:
  - Starting team: 9 agents
  - Other team: 8 agents
  - Neutral: 7
  - Assassin: 1

## 3. Roles
- Spymaster: Sees all types, provides one-word clue + number.
- Operatives: Attempt to guess matching words.

## 4. Turn Sequence
1. Spymaster provides a clue (word + number N  0).
2. Operatives guess 1..(N + 1) cards (stop early on mistake or voluntary end).
3. Wrong/fallback resolution:
   - Wrong team agent: reveal, turn ends, control switches.
   - Neutral: reveal, turn ends, control switches.
   - Assassin: reveal, opposing team wins immediately.
4. Win if a team reveals all its agents.

## 5. Clue Rules (Enforced Initial)
- Single word, letters only.
- Not identical to any unrevealed board word.
- Not a direct substring or superstring of an unrevealed word (simple rule).
- No spaces or punctuation.

## 6. Game Phases
- Setup
- AwaitingClue
- AwaitingGuesses
- Complete

## 7. Data Model (Abstract)
- Game(Id, Phase, CurrentTeam, Board, RedAgentsRemaining, BlueAgentsRemaining, ClueHistory, TurnHistory, CurrentTurn?)
- Board: 25 indexed GameCards
- GameCard: Position, Word, Type, IsRevealed
- Clue: Text, DeclaredCount, Team, Timestamp
- TurnRecord (introduced after Stage 5beyond initial Stage 1 scope)

## 8. Actions (Lifecycle)
- Create New Game (random or specified starting team; assign words & types)
- Submit Clue (if AwaitingClue)
- Make Guess (if AwaitingGuesses)
- End Turn
- Retrieve Game State
- Retrieve History

## 9. Error Conditions (High-Level)
- Invalid phase for action
- Invalid clue format
- Guessing already revealed card
- Index out of range (024 only)
- Game already complete

## 10. Out-of-Scope (Initial MVP)
- Authentication
- Persistence across restarts
- Real-time push (SignalR)
- Multi-board sizes
- Internationalization
- Rich morphological clue detection

## 11. API (Forthcoming)
Defined later in stages doc.

## 12. Quality Goals
- Deterministic board generation with seeded RNG (testable)
- 70% coverage of core logic before UI layering
- Exceptions mapped to consistent API error envelope later

## 13. Future Extensions
- Save/Load JSON
- Spectator mode
- Spymaster secret token
- Extended word lists
- Timer phases
