# Technical Specification

This is the technical specification for the spec detailed in @.agent-os/specs/2025-09-13-library-scan-backend/spec.md

## Technical Requirements

- Input
  - Source `libraryPath` comes from persisted settings (`api/config/settings.json`).
  - Require absolute path; validation errors should fail fast.

- Enumeration & Filtering
  - Recursively enumerate directories (streaming) starting at `libraryPath`.
  - Skip hidden/system directories where possible (best-effort via attributes).
  - Include files with `.mp3` extension (case-insensitive) only.

- Metadata Extraction
  - Use TagLib# (TagLibSharp) to read ID3 tags for `title` and `artist`.
  - Normalize: trim, collapse internal whitespace.
  - Fallback: for missing tags, parse filename pattern `Artist - Title.mp3` (best-effort).

- Record Structure & Identity
  - Fields: `id`, `title`, `artist`, `path`.
  - Deterministic `id`: SHA-256 of full path, hex string truncated to 16 bytes (32 hex chars).
  - Keep `path` as an absolute path from the server's perspective.

- Output & Durability
  - Write to `api/data/library.json` (content root `data/` directory).
  - Encode JSON as UTF-8 with camelCase fields.
  - Atomic write via temp file + replace/move; create `api/data/` if missing.
  - Optionally sort entries by `artist` then `title` for stable diffs.

- Resilience & Logging
  - Catch and skip unreadable/corrupt files; log per-file errors at Debug/Information, summary at Information.
  - Continue processing on errors; include total scanned, indexed, failed counts in logs.

- Performance
  - Streaming enumeration to avoid storing all file paths in memory.
  - Parallelize metadata extraction with a bounded degree (e.g., `Environment.ProcessorCount`) if simple to implement; otherwise, sequential is acceptable for MVP.
  - Avoid opening full audio streams; TagLib# should read header/tag regions only.

## External Dependencies (Conditional)

- NuGet: `TagLibSharp` — ID3 tag parsing for MP3 files.
  - Justification: Widely used, actively maintained; avoids implementing binary parsing.
  - Version: latest compatible with .NET 9 (exact version pinned during implementation).

