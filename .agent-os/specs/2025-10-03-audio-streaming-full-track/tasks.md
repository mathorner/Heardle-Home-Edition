# Spec Tasks

- [ ] 1. Track lookup & session payloads
  - [ ] 1.1 Add unit tests for resolving `trackId` → `TrackRecord` (ready, missing, library not ready)
  - [ ] 1.2 Implement lookup helper/service and ensure cache reuse with `ILibraryIndexProvider`
  - [ ] 1.3 Extend game session view/start responses with `trackId` and `audioUrl` fields (no leakage of title/artist)
  - [ ] 1.4 Update existing backend tests/fixtures to cover the new DTO contract
  - [ ] 1.5 Run `dotnet test` for backend units

- [ ] 2. Audio streaming endpoint
  - [ ] 2.1 Write integration tests for `/api/audio/{trackId}` covering 200 full download, 206 range, 404 missing track, 503 library not ready, 410 missing file, and HEAD responses
  - [ ] 2.2 Implement streaming endpoint with `EnableRangeProcessing`, MIME detection, and structured logging
  - [ ] 2.3 Ensure error payloads follow `{ code, message }` shape and that missing files invalidate cached index entries when appropriate
  - [ ] 2.4 Run backend integration tests (`dotnet test` relevant projects)

- [ ] 3. Frontend audio integration
  - [ ] 3.1 Update `gameClient` typings/parsers to include `trackId`/`audioUrl`, adjust mocks/tests
  - [ ] 3.2 Enhance `Game` component with `<audio>` controls, loading/error states, and rehydration logic for audio source
  - [ ] 3.3 Add/extend component tests ensuring audio wiring, range error handling, and sessionStorage clearing on failures
  - [ ] 3.4 Run `npm test -- --run` (or documented command) for frontend suite

- [ ] 4. Verification & documentation
  - [ ] 4.1 Smoke test full playback in browser (start game, play, scrub, refresh to confirm persistence)
  - [ ] 4.2 Update README and Product Plan with Iteration 6 status and `/api/audio/{trackId}` contract
  - [ ] 4.3 Run full project test commands (`dotnet test`, `npm test -- --run`) and capture results

