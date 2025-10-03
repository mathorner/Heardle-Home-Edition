# Spec Requirements Document

> Spec: Audio Streaming (Full Track)
> Created: 2025-10-03

## Overview

Introduce full-track audio streaming so the frontend can play and scrub through songs drawn from a user's scanned library. The backend must expose a range-enabled endpoint that serves MP3 (and other supported formats) directly from disk while enforcing the existing library readiness checks. Game sessions should surface a safe identifier that lets the UI request audio without revealing the track metadata. The frontend will wire this endpoint into the Play view to validate playback ahead of snippet orchestration.

## User Stories

### Stream an indexed track to the browser
As a player, I want the web app to stream the currently selected song from my library so I can listen and scrub like a normal audio player.

Details: When a game starts, the UI can request `/api/audio/{trackId}` and receive a range-enabled response that works with the HTML `<audio>` element controls.

### Fail gracefully when audio is unavailable
As a player, I need clear feedback if the server cannot stream the song (missing file, library not scanned) so I understand what action to take.

Details: The audio endpoint returns structured JSON errors for missing tracks or unreadable library indexes and the UI surfaces actionable messages.

### Rehydrate audio after refreshing the page
As a returning player, I want my active game's audio stream to remain available after a reload so I can continue listening without restarting the round.

Details: The persisted `gameId` should rehydrate both the session metadata and the associated `trackId`, allowing the UI to reconstruct the audio source URL automatically.

## Spec Scope

1. **Track lookup & session payload updates**
   - Extend the library index plumbing with an efficient lookup (e.g., dictionary projection or helper service) to resolve a `trackId` to its `TrackRecord` while honoring the existing cache semantics.
   - Update the game session DTOs and API responses so `startGame`/`getGameSession` include a stable `trackId` (hash) and an `audioUrl` pointing at `/api/audio/{trackId}`. Ensure the identifier exposes no title/artist data.
   - Revise backend tests to cover the new fields and confirm they remain absent from error responses.

2. **Audio streaming endpoint**
   - Add `GET /api/audio/{trackId}` (and implicit HEAD) that loads the requested track via the lookup helper, validates the file still exists, and returns a `FileStreamResult`/`PhysicalFileResult` with `EnableRangeProcessing = true` and the correct `Content-Type` (default `audio/mpeg`, fallback by extension).
   - Return JSON errors for: library not ready (503), track not found in index (404), underlying file missing or unreadable (410/404), and unexpected failures (500). Emit `Accept-Ranges: bytes`, `Last-Modified`, and accurate `Content-Length` / `Content-Range` headers so scrubbing works.
   - Instrument with structured logs for streaming start/stop and access failures without leaking file paths to clients.

3. **Frontend audio integration**
   - Update `gameClient` types to parse `trackId`/`audioUrl`, adjust tests, and ensure stale sessions clear storage when no audio is available.
   - Enhance the `Game` component to render an `<audio>` element (controls enabled, `preload="metadata"`, `playsInline`) wired to the session's audio URL. Display helpful status texts (loading, missing audio, retry prompt) based on endpoint responses.
   - Handle rehydration by reusing the persisted `gameId`, refetching the session, and reconnecting the audio source without user interaction besides the required play gesture.

4. **Testing & documentation**
   - Add integration tests covering full-file (200) and ranged (206) responses, invalid `trackId`, missing files, and HEAD requests. Validate headers relevant to range playback.
   - Create frontend unit/component tests to assert audio source wiring, error overlays, and rehydration flows.
   - Update README and Product Plan to reflect Iteration 6 status, the new endpoint contract, and any setup prerequisites (e.g., confirm FFmpeg not yet required).

## Out of Scope

- Server-side snippet trimming or FFmpeg integration (planned Iteration 10).
- Guess validation logic, attempt progression, or reveal flow (Iterations 7-9).
- Authentication, authorization, or DRM on audio streams.
- Advanced caching/CDN configuration beyond the minimal headers needed for browser playback.

## Expected Deliverable

1. Backend endpoint `/api/audio/{trackId}` streams indexed files with full range support, clear error handling, and comprehensive tests.
2. Game session APIs surface `trackId`/`audioUrl`, enabling the frontend to resume audio playback across refreshes without exposing answer metadata.
3. The Play view renders a working audio player that can start, pause, and scrub the active track using the new endpoint, with UX fallbacks for failure cases.
4. Documentation and automated tests are updated to describe and validate the new audio streaming capabilities.

