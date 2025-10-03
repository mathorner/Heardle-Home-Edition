# Technical Specification

This is the technical specification for the spec detailed in @.agent-os/specs/2025-10-03-audio-streaming-full-track/spec.md

## Technical Requirements

- Track lookup helper
  - Add `ITrackLookup` (or extend `ILibraryIndexProvider`) with `ValueTask<TrackRecord?> FindByIdAsync(string trackId, CancellationToken ct)` that reuses the cached snapshot when ready.
  - Validate `trackId` formatting (32 hex characters). Return `null` for missing tracks and propagate `LibraryNotReady` status via a structured result so callers can translate to HTTP codes.
  - Consider materializing a dictionary keyed by `TrackRecord.Id` when loading the snapshot to prevent O(n) lookups per request for large libraries.
  - Update `GameSessionView` and serialization DTOs to include `trackId` and `audioUrl`. Ensure `audioUrl` is generated server-side as `"/api/audio/{trackId}"` to avoid duplication in the frontend.
  - Adjust existing mappers/tests (e.g., `GameEndpointsTests`, `GameSessionStoreTests`) for the additional properties.

- Audio streaming endpoint implementation
  - Create `AudioEndpoints` module mapped under `/audio` and register from `Program.cs` similar to `GameEndpoints`.
  - Endpoint algorithm:
    1. Validate `trackId` (length/characters); return 400 JSON on failure.
    2. Retrieve snapshot via lookup helper; if snapshot status is `NotReady` return 503 error.
    3. When the track is not present, return 404 JSON (`TrackNotFound`).
    4. Confirm the underlying file exists; if missing, log at Warning, optionally call `ILibraryIndexProvider.Invalidate()`, and return 410 JSON.
    5. Infer MIME (`audio/mpeg` default; inspect extension for `audio/aac`, `audio/flac`, etc.).
    6. Use `Results.File(path, contentType, enableRangeProcessing: true, lastModified: File.GetLastWriteTimeUtc(path))` to produce `FileStreamResult`/`PhysicalFileResult` so ASP.NET manages 200/206/HEAD semantics.
    7. Wrap streaming in `try/catch` to surface 500 JSON with code `ServerError`.
  - Implement HEAD handling by relying on ASP.NET's built-in behavior; ensure tests assert no body is returned.
  - Add logging via `ILogger` with structured properties (`TrackId`, `FileSize`, `RangeStart/End`, `StatusCode`). Avoid logging full file paths at Information level.

- Frontend integration
  - Update `GameSession` type to carry `trackId` and `audioUrl`; tighten parsing to ensure both fields are present.
  - Introduce an `AudioPlayer` subcomponent (optional) that accepts `src` and `loading/error` props. Use `<audio controls playsInline preload="metadata">` and expose a retry button when the source fails.
  - In the `Game` component, when a session exists, render the audio player once the `audioUrl` is available. Provide a "Tap to enable audio" helper message for mobile (without implementing full unlock flow yet).
  - Handle fetch errors from `/api/audio/{trackId}` by listening to the `<audio>` element's `onError` event and surfacing a CTA (e.g., to rescan or retry start). Do not crash the session state.
  - Persist `trackId` alongside `gameId` in `sessionStorage` so rehydration can short-circuit audio setup while the session fetch resolves.
  - Update Vitest tests (`Game.test.tsx`, `gameClient.test.ts`) to cover the new contract and UI behaviors.

- Testing strategy
  - Backend: extend integration tests using `TestWebApplicationFactory` with temp media files to assert 200/206 responses, HEAD requests, invalid range (416), missing track (404), and library-not-ready (503). Validate `Accept-Ranges` and `Content-Type` headers.
  - Backend: unit-test MIME detection helper and lookup edge cases (invalid hash, missing file triggers invalidation).
  - Frontend: simulate resolved session to ensure audio player renders with correct `src`, verify rehydration uses stored `trackId`, and assert error messaging when `startGame` returns without audio.
  - Manual QA: start game, play audio, scrub near end, reload page, confirm playback resumes after a user gesture, and test behavior when deleting the underlying file mid-session.

- Documentation & DevEx
  - README: add instructions for placing sample MP3s, mention that audio streaming now works and requires supported formats.
  - Product Plan: mark Iteration 6 deliverable as in-progress/completed with bullet summary.
  - Optionally provide a `curl` example for ranged requests in docs.

## External Dependencies (Conditional)

- No new third-party dependencies are required. If MIME sniffing needs enhancement, prefer the built-in `FileExtensionContentTypeProvider` from `Microsoft.AspNetCore.StaticFiles` (already part of ASP.NET shared framework).

