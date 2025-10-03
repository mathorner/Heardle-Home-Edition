# API Specification

This is the API specification for the spec detailed in @.agent-os/specs/2025-10-03-audio-streaming-full-track/spec.md

## Endpoints

### POST /api/game/start

**Purpose:** Start a new game session and return metadata required to stream audio for the selected track.
**Parameters:** None (body empty)
**Response:**
- 201 Created
```
{
  "gameId": "7a11c0dc-8b7a-43c8-921c-1bd5c620c7a2",
  "status": "active",
  "attempt": 1,
  "maxAttempts": 6,
  "trackId": "3f4e6c2d10a34971bb0f1d9eb5523c4f",
  "audioUrl": "/api/audio/3f4e6c2d10a34971bb0f1d9eb5523c4f",
  "createdAt": "2025-10-03T09:12:41.281Z",
  "updatedAt": "2025-10-03T09:12:41.281Z"
}
```
- Headers: `Location: /api/game/{gameId}`
**Errors:**
- 503 Service Unavailable `{ "code": "LibraryNotReady", "message": "Run a scan before playing." }`
- 409 Conflict `{ "code": "NoTracksIndexed", "message": "No tracks available. Scan your library." }`
- 500 Internal Server Error `{ "code": "ServerError", "message": "Unable to start game." }`

### GET /api/game/{id}

**Purpose:** Retrieve an existing game session including its audio stream reference for rehydration.
**Parameters:**
- `id` (path) — GUID of the game session
**Response:**
- 200 OK
```
{
  "gameId": "7a11c0dc-8b7a-43c8-921c-1bd5c620c7a2",
  "status": "active",
  "attempt": 1,
  "maxAttempts": 6,
  "trackId": "3f4e6c2d10a34971bb0f1d9eb5523c4f",
  "audioUrl": "/api/audio/3f4e6c2d10a34971bb0f1d9eb5523c4f",
  "createdAt": "2025-10-03T09:12:41.281Z",
  "updatedAt": "2025-10-03T09:12:41.281Z"
}
```
**Errors:**
- 400 Bad Request `{ "code": "InvalidGameId", "message": "Game id must be a GUID." }`
- 404 Not Found `{ "code": "GameNotFound", "message": "Game session not found." }`
- 500 Internal Server Error `{ "code": "ServerError", "message": "Unable to load session." }`

### GET /api/audio/{trackId}

**Purpose:** Stream an indexed audio file with byte-range support for HTML `<audio>` playback.
**Parameters:**
- `trackId` (path) — Deterministic hash identifier of the requested track
- Headers: optional `Range`, `If-Modified-Since`, `If-Range`
**Response:**
- 200 OK (no `Range` header)
  - Body: full audio file stream (`audio/mpeg` by default)
  - Headers: `Accept-Ranges: bytes`, `Content-Length`, `Last-Modified`
- 206 Partial Content (when `Range` supplied)
  - Body: requested byte slice
  - Headers: `Content-Range`, `Accept-Ranges: bytes`, `Content-Length`
**Errors:**
- 400 Bad Request `{ "code": "InvalidTrackId", "message": "Track id must be a 32 character hash." }`
- 404 Not Found `{ "code": "TrackNotFound", "message": "Track not found in index." }`
- 410 Gone `{ "code": "TrackFileMissing", "message": "Audio file is no longer available." }`
- 416 Range Not Satisfiable `{ "code": "InvalidRange", "message": "Requested range is not satisfiable." }`
- 503 Service Unavailable `{ "code": "LibraryNotReady", "message": "Run a scan before playing." }`
- 500 Internal Server Error `{ "code": "ServerError", "message": "Unable to stream audio." }`

**Notes:** HEAD requests must return the same headers without a body. Unexpected exceptions should log and return a generic 500 JSON payload.

