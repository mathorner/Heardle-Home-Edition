# API Specification

This is the API specification for the spec detailed in @.agent-os/specs/2025-09-12-project-scaffolding/spec.md

## Endpoints

### GET /health

Purpose: Report service availability/status for the frontend and monitors.
Parameters: none
Response: `200 OK`, `application/json` body, e.g. `{ "status": "ok" }`
Errors: `5xx` on unexpected failures

