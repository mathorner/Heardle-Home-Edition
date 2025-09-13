# Technical Specification

This is the technical specification for the spec detailed in @.agent-os/specs/2025-09-13-library-path-input/spec.md

## Technical Requirements

- Backend (.NET 9 Minimal API)
  - Endpoint: `POST /api/settings/library-path`
    - Request: `{ "path": string }`
    - Validation: must be absolute path, directory exists, and is readable by the server process.
    - On success: persist to `config/settings.json` as `{ "libraryPath": "<absolute>" }` and return 200 with `{ saved: true, path: "<absolute>" }`.
    - On failure: return `400 Bad Request` with `{ saved: false, code: "InvalidPath|NotFound|AccessDenied|Unknown", message: string }`.
  - Normalization: use `Path.GetFullPath` and require `Path.IsPathRooted(path)`; reject relative paths.
  - Readability checks: `Directory.Exists(path)`, attempt to enumerate with minimal access (e.g., `Directory.EnumerateFileSystemEntries(path).Take(0)`), catching `UnauthorizedAccessException`.
  - Persistence: ensure `config/` exists; write atomically (`File.WriteAllText` to temp + move, or `File.Replace`) to avoid partial writes.
  - Security: never list or return file names; do not follow untrusted symlinks; do not expand `~`.
  - Environment: CORS remains Development-only; HTTPS/HSTS enabled in non-Development (already configured in Program.cs).

- Frontend (React + TypeScript)
  - Settings UI: a simple form (input[type=text] + Save button) to submit the path.
  - UX: show success confirmation on 200; show error summary using `role="alert"` on 400 with message.
  - Config: use `/api` proxy in dev; production can supply `VITE_API_BASE_URL`.

- Data
  - File: `config/settings.json` at API app root.
  - Shape: `{ "libraryPath": "<absolute path>" }`.

## External Dependencies (Conditional)

No new external dependencies required. Use built-in `System.IO` APIs.

