# Technical Specification

This is the technical specification for the spec detailed in @.agent-os/specs/2025-09-25-random-track-game-session/spec.md

## Technical Requirements

- Library Index Provider
  - Implement `ILibraryIndexProvider` with methods:
    - `Task<LibraryIndexSnapshot> GetAsync(CancellationToken ct)` → returns cached tracks plus metadata (`LastLoadedAt`, `TotalTracks`).
    - `void Invalidate()` → clear cache so the next `GetAsync` reloads from disk.
  - On load, read `{contentRoot}/data/library.json`; deserialize to `TrackRecord[]` (reuse existing model).
  - Skip records whose `path` no longer exists; log at Information level when the file is missing.
  - Cache tracks in memory; keep `LastWriteTimeUtc` of the file and reload when it changes or on explicit invalidation.
  - Update `LibraryScanService` to call `Invalidate()` after a successful write so new scans are reflected immediately.

- Random Track Selection
  - Use `RandomNumberGenerator.GetInt32` (or injectable `IRandomSource`) for unbiased selection.
  - Track the last served `TrackId` and reroll once when the library contains >1 track to avoid instant repeats.
  - Return `NoTracksIndexed` when the filtered list is empty; return `LibraryNotReady` when the index file is missing or unreadable.

- Game Session Store
  - Define `GameSession` record/class with:
    - `Guid Id`, `string TrackId`, `string TrackPath`, `string Title`, `string Artist`, `int Attempt`, `int MaxAttempts = 6`, `string Status`, `DateTimeOffset CreatedAt`, `DateTimeOffset UpdatedAt`.
    - `Status` values: `active`, `won`, `lost`.
  - Implement `InMemoryGameSessionStore` using `ConcurrentDictionary<Guid, GameSession>`.
  - Provide methods:
    - `GameSession Create(TrackRecord track)` → inserts new session, returns instance.
    - `bool TryGet(Guid id, out GameSession session)` → returns stored session.
    - `void Touch(Guid id, Func<GameSession, GameSession> mutate)` → update helper for future iterations.
  - Add a background timer (or lazy sweep on access) to remove sessions older than 2 hours; log removals at Debug.

- API Endpoints (`GameEndpoints`)
  - POST `/game/start`
    - Inject `ILibraryIndexProvider`, `IGameSessionStore`, and `ILogger<GameEndpoints>`.
    - Load index; handle provider error codes with mapped HTTP status (503 / 409) and JSON payload `{ code, message }`.
    - Select random track, create session, and return 201 with body `{ gameId, status, attempt, maxAttempts, createdAt }` and `Location` header.
  - GET `/game/{id}`
    - Validate `Guid` route parameter; return 400 on parse failure.
    - Lookup session; if found, return sanitized view `{ gameId, status, attempt, maxAttempts, createdAt, updatedAt }`.
    - If missing/expired, return 404 `{ code: 'GameNotFound', message: 'Game session not found.' }`.
  - Extend integration tests to cover:
    - Success path (start → fetch) with temp index file.
    - Missing index (503) and empty tracks (409).
    - GET 404 for unknown IDs and 400 for non-Guid route.

- Frontend Client & UI
  - Add `frontend/src/lib/gameClient.ts` exporting:
    - `startGame()` → POST `/api/game/start`; parse 201 body; surface structured errors for 503/409 with `code`/`message` fields.
    - `getGameSession(id)` → GET `/api/game/{id}`; return session view; throw on unexpected failures.
  - Update `frontend/src/App.tsx` nav to include "Play" (default view) → renders new `Game` component.
  - Implement `Game` component:
    - Manage `gameId`, `status`, `attempt` state.
    - On mount, read `sessionStorage.getItem('activeGameId')`; fetch session when present.
    - Provide "Start Game" button that calls `startGame`, stores `gameId`, updates state, and handles loading/disabled states.
    - Render status line (`Attempt 1 of 6`, `Game over` placeholder) in an `aria-live="polite"` region.
    - Show actionable errors (buttons/links) for `LibraryNotReady` and `NoTracksIndexed` (e.g., button navigates to Settings view via existing state).
  - Write component tests with mocked client to cover start success, resume existing game, handle 404 clearing storage, and error display.

- Logging & Telemetry
  - Log game creation at Information level with session id and track id (no title/artist) for diagnosis.
  - Log provider errors and expired session cleanup at Debug.

- Documentation
  - Update `README.md` current status (Iteration 5) and document new endpoints and Play flow.
  - Update `Planning/Product-Plan.md` to mark Iteration 5 deliverables as complete when done.

## External Dependencies (Conditional)

- None required beyond the existing stack.

