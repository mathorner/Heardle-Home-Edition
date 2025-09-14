# Spec Tasks

- Spec: Library Scan (Backend)
- Scope: Recursively index MP3s from saved `libraryPath`, extract `title`/`artist` (TagLib# with filename fallback), and write `api/data/library.json` atomically with deterministic IDs.

## Tasks

- [ ] 1. Scan service scaffolding + helpers
  - [x] 1.1 Write tests for helpers: deterministic ID from path (SHA-256 → 32 hex chars) and filename fallback parser (`Artist - Title.mp3`)
  - [x] 1.2 Define models (TrackRecord { id, title, artist, path }) and normalization helpers (trim/collapse whitespace)
  - [x] 1.3 Implement recursive directory enumeration with streaming and `.mp3` filter (case-insensitive)
  - [x] 1.4 Verify all tests pass

- [ ] 2. TagLib# integration for metadata
  - [x] 2.1 Add `TagLibSharp` NuGet dependency to `api`
  - [x] 2.2 Implement metadata extractor: title/artist via TagLib; fallback to filename when missing
  - [x] 2.3 Normalize strings (trim/collapse) and guard against invalid/corrupt files with try/catch
  - [x] 2.4 Verify all tests pass

- [ ] 3. Index writer (atomic JSON output)
  - [x] 3.1 Ensure `api/data/` exists; write `data/library.json` with camelCase fields via temp file + replace/move
  - [x] 3.2 Optional: sort by artist, then title for stable diffs
  - [x] 3.3 Write unit test for serializer shape (id/title/artist/path)
  - [x] 3.4 Verify all tests pass

- [ ] 4. Scan orchestration + resilience
  - [x] 4.1 Load saved `libraryPath`; fail fast if missing/invalid (log and return)
  - [x] 4.2 Stream enumeration → extract metadata → build TrackRecord with deterministic id
  - [x] 4.3 Continue past errors; collect counts for total/indexed/failed; log summary at Information level
  - [x] 4.4 Save final index atomically to `data/library.json`
  - [x] 4.5 Verify all tests pass

- [ ] 5. Dev wiring and docs
  - [x] 5.1 Add `api/data/.gitkeep` and ignore `api/data/library.json` in VCS
  - [x] 5.2 Update README with where the index is stored and note that triggering/status endpoints arrive in Iteration 4
  - [x] 5.3 Verify all tests pass
