# Overall Architecture (High-Level)

## 1. Layers
1. Core (Pure Domain)
2. API (Delivery)
3. Web Front-End (Static)
4. Tests

## 2. Dependency Direction
Front-End  API  Core; Tests reference Core (later API). No reverse coupling.

## 3. Core Composition
WordProvider (later)  BoardGenerator  GameService  ClueValidator (later)

## 4. Principles
Immutability where feasible, explicit phase transitions, DTO isolation, fail-fast validation.

## 5. Error Handling
Custom exceptions in Core mapped to JSON errors in API.

## 6. Scalability Path
Introduce persistence & repository abstraction later.

## 7. Security Placeholder
Local only now; token for spymaster later.

## 8. Testing Strategy
Unit first, integration once API endpoints appear (Stage 8+).

## 9. Extensibility
Interfaces for word source, validator strategies, persistence adapter.

## 10. Risks
Validator complexity, randomness unpredictability  mitigated via seeds & tests.

## 11. Performance
Negligible resource usage; easily extendable.
