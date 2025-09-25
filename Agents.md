# Agents Kickoff Guide

## Purpose
Start here to orient yourself. This file points you to the living sources of truth so you can pick up work without needing this document to be updated after every change.

## Key References (read in order)
1. `Planning/Product-Plan.md` – product vision, roadmap, and iteration objectives.
2. `.agent-os/specs/` – feature specs with `spec.md` + `tasks.md` per iteration; use task checkboxes to infer progress.
3. `Planning/Initial Prompt.md` – original request and MVP acceptance criteria.
4. `.agent-os/instructions/` & `.agent-os/standards/` – Agent OS workflows, coding standards, and best practices.
5. `README.md` – current local development setup, tooling commands, and project overview.

## Repository Landmarks
- Backend (`api/`) – ASP.NET Core minimal API. Start with `api/Program.cs`, services under `api/Services/`, models in `api/Models/`, endpoints in `api/Endpoints/`.
- Backend tests (`api.tests/`) – xUnit integration/unit tests; `Infrastructure/TestWebApplicationFactory.cs` configures temp content root.
- Frontend (`frontend/`) – Vite + React + TypeScript. Entry is `frontend/src/App.tsx`; components and clients under `frontend/src/components/` and `frontend/src/lib/`; Vitest setup in `frontend/src/setupTests.ts`.
- Configuration & data directories are created under `api/config/` and `api/data/` (ignored by git). Consult the relevant spec before modifying persistence or indexes.

## How to Get In Sync
- Review the latest iteration specs inside `.agent-os/specs/` to understand active work. Each spec folder is date-stamped; `tasks.md` reflects execution status.
- Cross-check the roadmap in `Planning/Product-Plan.md` to see upcoming iterations and dependencies.
- Inspect recent git history or open pull requests if additional context is needed (outside this doc).

## Working Notes
- Follow the Agent OS command flow described in `.agent-os/instructions/` when generating or executing new specs/tasks.
- Prefer to run build/test commands documented in `README.md`; if tooling changes, update README rather than this file.
- Adhere to the coding guidelines in `.agent-os/standards/` for style, simplicity, and dependency selection.

With these pointers you should have the current context; consult the source docs above for details and status before making changes.
