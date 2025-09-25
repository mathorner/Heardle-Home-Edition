# API Specification

This is the API specification for the spec detailed in @.agent-os/specs/2025-09-13-scan-ui-progress/spec.md

## Endpoints

### POST /api/library/scan

Purpose: Kick off a background library scan if not already running.
Response:
- 202 Accepted: `{ "status": "running", "startedAt": "2025-09-13T12:34:56Z" }`
- 409 Conflict: `{ "status": "running" }` (scan already in progress)
- 400 Bad Request: `{ "code": "MissingLibraryPath", "message": "Library path is not configured" }`

### GET /api/library/status

Purpose: Report current scan status and counts.
Response 200 OK:
```
{ "status": "idle|running|completed",
  "total": 0,
  "indexed": 0,
  "failed": 0,
  "startedAt": "2025-09-13T12:34:56Z",
  "finishedAt": "2025-09-13T12:35:56Z" }
```

