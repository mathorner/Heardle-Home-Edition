# API Specification

This is the API specification for the spec detailed in @.agent-os/specs/2025-09-13-library-path-input/spec.md

## Endpoints

### POST /api/settings/library-path

Purpose: Save the absolute path to the user's music library.
Parameters: JSON body `{ "path": string }`
Response:
- 200 OK, `application/json`: `{ "saved": true, "path": "<absolute>" }`
- 400 Bad Request, `application/json`: `{ "saved": false, "code": "InvalidPath|NotFound|AccessDenied|Unknown", "message": "..." }`
Errors: Use specific `code` values to aid the UI; never disclose file listings.

