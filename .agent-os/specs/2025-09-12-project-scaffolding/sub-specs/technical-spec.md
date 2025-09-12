# Technical Specification

This is the technical specification for the spec detailed in @.agent-os/specs/2025-09-12-project-scaffolding/spec.md

## Technical Requirements

- Backend (.NET 9 Minimal API)
  - Expose `GET /health` returning HTTP 200 and JSON body, e.g. `{ "status": "ok" }`.
  - Enable CORS for the local frontend origin during development.
  - Basic logging; no persistence or other endpoints in this iteration.

- Frontend (Vite + React + TypeScript)
  - Single route `/` with a page that renders the app title and the backend health status.
  - Fetch `/health` from the backend and display status or a clear error state.
  - Include mobile viewport meta and base responsive styles with design tokens (colors, spacing, typography scale).
  - Keep bundle minimal; no router beyond what is needed for `/`.

- Integration
  - Local dev: either configure Vite dev proxy to the backend or set a `VITE_API_BASE_URL` env var and use it for fetches.
  - Confirm CORS allows frontend→backend requests in dev.

- Performance & Accessibility (baseline)
  - Lightweight initial render (<100ms on desktop dev machines is acceptable at this stage).
  - Provide accessible text contrast and focus styles.

## External Dependencies

- Frontend
  - React 18, React DOM
  - Vite, TypeScript

- Backend
  - ASP.NET Core (.NET 9) Minimal APIs (no extra packages beyond defaults)

**Justification:** These are standard, minimal dependencies to scaffold a modern, type-safe web app with a health-checked API and a React SPA front end.

