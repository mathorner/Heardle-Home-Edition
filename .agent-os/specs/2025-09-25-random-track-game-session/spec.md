# Spec Requirements Document

> Spec: Random Track + Game Session
> Created: 2025-09-25

## Overview

Enable players to start a Heardle round backed by an in-memory game session that selects a random track from the scanned library index. The backend must manage session state (track selection, attempt counter, status) and expose endpoints for the frontend to start and rehydrate a session. The frontend adds a "Play" experience that can initiate a round and surface helpful error states when the library is not ready.

## User Stories

### Start a new round with a random track
As a player, I want to tap "Play" and immediately start a game that uses a random song from my indexed library so I can begin guessing.

Details: Backend chooses a random track from `data/library.json`, stores it in a server-side session, and returns a `gameId`, attempt number, and maximum attempts.

### Resume the current round after reload
As a player, I want the app to recover my active round if I refresh or reopen the tab so I do not lose progress midway through guessing.

Details: Frontend persists the `gameId` (e.g., sessionStorage) and calls a session lookup endpoint to restore attempt and status metadata without exposing the answer.

### Understand when the library is not playable yet
As a user, I want clear feedback when I try to start a game but the library has not been scanned (or is empty) so I know to run a scan first.

Details: Backend returns a structured error code/message; frontend shows an actionable prompt (e.g., link to Settings → Scan) instead of failing silently.

## Spec Scope

1. **Library index loading & random selection**
   - Implement a lightweight service (e.g., `ILibraryIndexProvider`) that loads `api/data/library.json`, caches the parsed tracks in memory, and refreshes when the file timestamp changes or when explicitly invalidated after a scan.
   - Track schema: `{ id, title, artist, path }`; ignore entries whose files no longer exist; log skips but continue.
   - Provide `TryGetRandomTrack()` that returns a uniformly random track (avoid immediate repeats of the last selected track when possible).
   - Surface `LibraryNotReady` (index missing/unreadable) and `NoTracksIndexed` (zero usable tracks) error codes for callers.

2. **Game session domain & storage**
   - Define `GameSession` model with fields: `Id (Guid)`, `TrackId`, `TrackPath`, `Title`, `Artist`, `Attempt` (starts at 1), `Status` (`active|won|lost`), `MaxAttempts` (constant 6), `CreatedAt`, `UpdatedAt`.
   - Implement `IGameSessionStore` as an in-memory, thread-safe cache keyed by `Id`, supporting `Create`, `GetSnapshot`, and `UpdateAttempt/Status` (future use). Include an expiration sweep (~2 hours) to avoid unbounded growth.
   - Store only server-visible data (path/title/artist) internally; expose sanitized snapshots to the API that omit the answer until reveal.

3. **API endpoints**
   - POST `/api/game/start`
     - Behavior: load (or refresh) index via provider, pick random track, create session, and return 201 Created.
     - Response body: `{ gameId, status: 'active', attempt: 1, maxAttempts: 6, createdAt }`.
     - Headers: `Location: /api/game/{gameId}`.
     - Error responses:
       - 503 `{ code: 'LibraryNotReady', message: 'Run a scan before playing.' }`
       - 409 `{ code: 'NoTracksIndexed', message: 'No tracks available. Scan your library.' }`
   - GET `/api/game/{id}`
     - Returns 200 with sanitized snapshot `{ gameId, status, attempt, maxAttempts, createdAt, updatedAt }`.
     - Returns 404 when the session is missing or expired.
   - Wire endpoints in `Program` via `GameEndpoints` module; ensure tests cover success, missing index, empty library, and invalid session id paths.

4. **Frontend play experience**
   - Add `Game` (or `Play`) view with prominent "Start Game" button on the home route; integrate into existing nav.
   - Create `gameClient.ts` with `startGame()` and `getGameSession(gameId)` helpers mirroring backend contracts (include tests with mocked `fetch`).
   - When starting succeeds: store `gameId` in `sessionStorage`, render attempt/status info, and show a placeholder message for upcoming audio playback.
   - On load: if a stored `gameId` exists, call lookup endpoint to rehydrate; handle 404 by clearing storage and showing "Start Game" button.
   - Display actionable errors for `LibraryNotReady`/`NoTracksIndexed`, guiding users to the Settings → Scan UI.
   - Ensure UI remains mobile-first (single-column stack, 44px targets) and accessible (`aria-live` updates for status changes).

## Out of Scope

- Streaming audio snippets or full tracks (Iterations 6-7).
- Guess submission, validation, or reveal logic (Iterations 8-9).
- Persisting sessions across server restarts or for multiple users.
- UI polish beyond basic layout and feedback messaging.

## Expected Deliverable

1. Backend session service and endpoints allow starting and retrieving game sessions backed by the scanned library index, with clear errors when unavailable.
2. Frontend "Play" experience can start a round, remember the session during the tab lifetime, and surface actionable error states.
3. Automated tests cover session service behavior, endpoint contracts (success and failure cases), and the frontend client/component flows.

