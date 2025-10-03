# Heardle Home Edition

Personalized Heardle-style music guessing game that uses your own music library (MP3, M4A, FLAC, and other common formats). Players listen to progressively longer snippets and guess the song (title + artist) within 6 attempts.

## Overview
- Backend: .NET 9 Minimal APIs (C#)
- Frontend: React + TypeScript (Vite)
- Audio: HTML5 Audio; later FFmpeg for exact-length snippets
- Platform: Mobile-first web app (phone and tablet friendly)

## Current Status
- Iteration 5 (Random Track + Game Session) active — backend game session endpoints, random track selection, Swagger docs, and frontend Play view delivered
- Latest spec: `.agent-os/specs/2025-09-25-random-track-game-session/`

## Roadmap (Phase 1 Highlights)
1) Iteration 1: Project Scaffolding — minimal API (`/health`), Vite React app, CORS, mobile baseline
2) Iteration 2: Settings — library path input and validation
3) Iteration 3–10: Library scan, game session, audio endpoints, client snippets → server-side exact snippets

See full plan: `Planning/Product-Plan.md`

## Local Development
- Backend API (port 5158 by default)
  - `cd api && dotnet run`
  - Endpoint: `GET /health` → `{ "status": "ok" }`
  - CORS allows `http://localhost:5173`
- Frontend (port 5173)
  - `cd frontend && npm run dev`
  - Uses Vite proxy to `/api` or `VITE_API_BASE_URL`
  - Home page fetches and displays API health
- Tests
  - Backend: `dotnet test`
  - Frontend: `cd frontend && npm test -- --run`
  - Frontend e2e: `cd frontend && npm run test:e2e` (install browsers once with `npx playwright install`)

### Ports and Proxy
- Vite dev server runs at `http://localhost:5173`.
- API (Development) runs at `http://localhost:5158` (see `api/Properties/launchSettings.json`).
- Vite proxy forwards `/api/*` → `http://localhost:5158` (see `frontend/vite.config.ts`).
- Alternative: set `frontend/.env` with `VITE_API_BASE_URL=http://localhost:5158` and call the API directly (CORS already permits `http://localhost:5173`).

## Settings: Library Path (Iteration 2)
- UI: Settings page to enter your music library folder path and save it
- Endpoint: `POST /settings/library-path` (dev via `/api/settings/library-path`)
- Persisted to: `api/config/settings.json` on the server (not committed)
- Validations: absolute path, exists, readable by the server process
- Errors: returns a 400 with a code and message for invalid paths

## Library Scan (Iterations 3–4)
- Purpose: Index audio files under the saved library path and surface progress to the UI
- Supported formats: `.mp3`, `.m4a`, `.aac`, `.flac`, `.wav`, `.ogg`, `.wma`, `.aiff`, `.aif`
- Output: `api/data/library.json` — `[ { id, title, artist, path } ]`
- Backend endpoints:
  - `POST /api/library/scan` → starts a scan when the library path is configured; returns `202` with current status or `409` if one is already running
  - `GET /api/library/status` → returns `{ status, total, indexed, failed, startedAt?, finishedAt? }`
- Frontend: Settings page now includes a "Scan Now" panel that triggers scans, polls every 750 ms while running, shows live counts, and reports conflicts/errors
- Behavior: Recursively enumerates supported formats, extracts metadata via TagLib# (fallback to filename `Artist - Title`), skips unreadable/corrupt files, writes output atomically

## Game Session (Iteration 5)
- Backend endpoints:
  - `POST /api/game/start` → selects a random indexed track, creates a session, returns `{ gameId, status, attempt, maxAttempts, createdAt }`; 503 when the index is missing, 409 when empty
  - `GET /api/game/{id}` → returns the active session snapshot or 404 when missing
- Session storage: In-memory cache with automatic expiry after 2 hours; cache invalidates when scans rebuild the library index
- Random selection avoids repeating the previous track when possible
- Frontend Play view (default tab) starts/resumes games, stores `gameId` in sessionStorage, and surfaces actionable errors pointing back to Settings → Scan

## API Explorer (Swagger)
- Run `cd api && dotnet run`
- Open `http://localhost:5158/swagger` to browse the OpenAPI UI generated via Swashbuckle

## Mobile UX Baseline
- Viewport meta configured for mobile
- Responsive layout (phone 320–414, tablet 768–1024)
- Touch-friendly targets (≥44×44px), high-contrast theme, focus styles

## Agent OS Usage
- Create a spec: `/create-spec <idea>` or `/create-spec` (interactive)
- Create tasks from approved spec: `/create-tasks`
- Execute tasks: `/execute-tasks` (focuses on Task 1 unless specified)
- Spec and tasks live under `.agent-os/specs/YYYY-MM-DD-slug/`

## Repository Structure (key paths)
- Planning/Product-Plan.md — roadmap and requirements
- .agent-os/instructions/** — Agent OS core flows
- .cursor/rules/** — IDE rule pointers to Agent OS flows
- .agent-os/specs/** — feature specs, tasks, and sub-specs

## License
TBD
