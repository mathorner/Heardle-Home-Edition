# Technical Specification

This is the technical specification for the spec detailed in @.agent-os/specs/2025-09-13-scan-ui-progress/spec.md

## Technical Requirements

- Backend Endpoints
  - POST `/api/library/scan`
    - Action: Start a background scan if not already running
    - Response:
      - 202 Accepted: `{ status: "running", startedAt }`
      - 409 Conflict: `{ status: "running" }` if already running
      - 400 Bad Request: `{ code: "MissingLibraryPath", message }` if no libraryPath configured
  - GET `/api/library/status`
    - Response 200 OK: `{ status: "idle|running|completed", total, indexed, failed, startedAt?, finishedAt? }`

- Progress Manager
  - In-memory singleton (e.g., `ScanManager`) that guards concurrent runs and captures progress counters.
  - Exposes thread-safe methods: `TryStart(Func<CancellationToken, Task<(int total,int indexed,int failed)>>)`, `GetStatus()`.
  - On completion, set `status = completed` and record `finishedAt` and final counts.

- Frontend
  - Add a Scan section to the UI with a "Scan Now" button.
  - On click: POST to `/api/library/scan`; if 202, start polling `/api/library/status` every ~750ms until `status === "completed"`.
  - While running: disable button; show `total/indexed/failed` and a live region.
  - On completion: show final counts and re-enable button.

- Accessibility & UX
  - Button remains large and reachable (≥44×44px).
  - Progress text is announced with `aria-live="polite"`.
  - Minimal layout shift; counts should be monospaced or reserved space to reduce jitter.

## External Dependencies (Conditional)

- None required beyond existing stack. SSE/WebSocket can be considered in a later iteration.

