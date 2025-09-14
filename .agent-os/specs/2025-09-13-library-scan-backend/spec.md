# Spec Requirements Document

> Spec: Library Scan (Backend)
> Created: 2025-09-13

## Overview

Implement a backend scan that recursively indexes MP3 files under the configured library path, extracts title and artist metadata, and writes a durable index for later features. The scan must be resilient, efficient for large libraries, and produce a consistent JSON format.

## User Stories

### Index my music library
As a player, I want my MP3s indexed so the game can later select tracks from my collection.

Details: Given a saved `libraryPath` in settings, scanning finds MP3 files in nested folders and records `id`, `title`, `artist`, and `path`.

### Keep scanning even if files fail
As a user, I want the scan to continue even when some files are missing/locked/corrupt so I get an index of all readable tracks.

Details: Errors are logged and skipped; the process reports total scanned, successes, and failures.

### Handle large libraries efficiently
As a user with 10k+ tracks, I want scanning to be reasonably fast and not exhaust memory.

Details: Stream directory enumeration; avoid loading all paths into memory at once; write output atomically at the end.

## Spec Scope

1. **Directory Enumeration** – Recursively enumerate from saved `libraryPath` (absolute), skip hidden/system directories where possible.
2. **File Filtering** – Include `.mp3` (case-insensitive); ignore other extensions for this iteration.
3. **Metadata Extraction** – Read ID3 tags for `title` and `artist` using TagLib#. Normalize whitespace; if missing, fallback to filename heuristics (`Artist - Title.mp3`).
4. **Record Structure** – Write `[{ id, title, artist, path }]` using a stable deterministic `id` derived from full path (e.g., SHA-256 hex truncated to 16 bytes).
5. **Output & Durability** – Write to `api/data/library.json` (UTF-8) with camelCase fields via atomic write (temp file + replace/move). Create `api/data/` if missing.
6. **Resilience & Logging** – Skip unreadable/corrupt files, log errors with counts, continue processing.

## Out of Scope

- Frontend UI and progress reporting (handled in Iteration 4).
- API endpoints to trigger or report status (Iteration 4).
- Non-MP3 formats, waveform/snippet generation.
- Database storage; JSON file only for this iteration.

## Expected Deliverable

1. Running the scan produces `api/data/library.json` containing entries for readable MP3s under `libraryPath`.
2. Scans nested folders; continues past errors; logs summary counts (total, indexed, failed).
3. Output JSON uses fields `id`, `title`, `artist`, `path` with deterministic `id`.

