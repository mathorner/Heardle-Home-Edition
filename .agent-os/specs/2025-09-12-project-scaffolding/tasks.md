# Spec Tasks

- Spec: Project Scaffolding
- Scope: .NET 9 Minimal API with `GET /health`, Vite React TS frontend fetching that status, dev CORS, and mobile-first baseline.

## Tasks

- [ ] 1. Backend minimal API with health endpoint and CORS
  - [x] 1.1 Write tests for API health endpoint (integration test verifies 200 and `{ "status": "ok" }`)
  - [x] 1.2 Scaffold .NET 9 Minimal API project (runnable locally)
  - [x] 1.3 Implement `GET /health` returning 200 and JSON body `{ "status": "ok" }`
  - [x] 1.4 Enable CORS for local frontend origin during development
  - [x] 1.5 Add basic logging configuration
  - [ ] 1.6 Verify all tests pass

- [ ] 2. Frontend scaffold with health status display
  - [x] 2.1 Write tests for Home view/component (renders title and fetched health status; mock fetch)
  - [x] 2.2 Scaffold Vite React + TypeScript app with dev script
  - [x] 2.3 Add mobile viewport meta and base design tokens (colors, spacing, type scale)
  - [x] 2.4 Implement fetch to API `/health` using `VITE_API_BASE_URL` or Vite proxy
  - [x] 2.5 Render health status and a clear error state in UI
  - [ ] 2.6 Verify all tests pass

- [ ] 3. Local dev wiring and documentation
  - [x] 3.1 Write tests for API client (mocked fetch path resolution and error handling)
  - [x] 3.2 Configure consistent local ports and/or Vite proxy to backend
  - [x] 3.3 Document quickstart (run backend, run frontend, env vars, ports)
  - [x] 3.4 Add basic accessibility pass (contrast, focus outlines)
  - [ ] 3.5 Verify all tests pass
