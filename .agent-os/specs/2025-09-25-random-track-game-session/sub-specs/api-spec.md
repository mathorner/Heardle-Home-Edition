# API Specification

This is the API specification for the spec detailed in @.agent-os/specs/2025-09-25-random-track-game-session/spec.md

## Endpoints

### POST /api/game/start

**Purpose:** Create a new game session by selecting a random track from the indexed library.
**Parameters:** None (body empty)
**Response:**
- 201 Created
```
{
  "gameId": "a3f5c2c4-5d0c-4b94-9c3e-a0a21a49cb61",
  "status": "active",
  "attempt": 1,
  "maxAttempts": 6,
  "createdAt": "2025-09-25T14:32:41.281Z"
}
```
- Headers: `Location: /api/game/{gameId}`
**Errors:**
- 503 Service Unavailable `{ "code": "LibraryNotReady", "message": "Run a scan before playing." }`
- 409 Conflict `{ "code": "NoTracksIndexed", "message": "No tracks available. Scan your library." }`
- 500 Internal Server Error `{ "code": "ServerError", "message": "Unable to start game." }`

### GET /api/game/{id}

**Purpose:** Retrieve a sanitized snapshot of an existing game session for UI rehydration.
**Parameters:**
- `id` (path) — GUID of the game session
**Response:**
- 200 OK
```
{
  "gameId": "a3f5c2c4-5d0c-4b94-9c3e-a0a21a49cb61",
  "status": "active",
  "attempt": 1,
  "maxAttempts": 6,
  "createdAt": "2025-09-25T14:32:41.281Z",
  "updatedAt": "2025-09-25T14:32:41.281Z"
}
```
**Errors:**
- 400 Bad Request `{ "code": "InvalidGameId", "message": "Game id must be a GUID." }`
- 404 Not Found `{ "code": "GameNotFound", "message": "Game session not found." }`
- 500 Internal Server Error `{ "code": "ServerError", "message": "Unable to load session." }`

