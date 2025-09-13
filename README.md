# Heardle Home Edition

Personalized Heardle-style music guessing game that uses your own MP3 library. Players listen to progressively longer snippets and guess the song (title + artist) within 6 attempts.

## Overview
- Backend: .NET 9 Minimal APIs (C#)
- Frontend: React + TypeScript (Vite)
- Audio: HTML5 Audio; later FFmpeg for exact-length snippets
- Platform: Mobile-first web app (phone and tablet friendly)

## Current Status
- Planning in progress via Agent OS
- Spec created for Iteration 1: Project Scaffolding
  - Path: `.agent-os/specs/2025-09-12-project-scaffolding/`
  - Tasks defined in `tasks.md`

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

### Ports and Proxy
- Vite dev server runs at `http://localhost:5173`.
- API (Development) runs at `http://localhost:5158` (see `api/Properties/launchSettings.json`).
- Vite proxy forwards `/api/*` → `http://localhost:5158` (see `frontend/vite.config.ts`).
- Alternative: set `frontend/.env` with `VITE_API_BASE_URL=http://localhost:5158` and call the API directly (CORS already permits `http://localhost:5173`).

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
