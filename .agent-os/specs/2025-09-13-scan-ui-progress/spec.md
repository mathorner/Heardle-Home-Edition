# Spec Requirements Document

> Spec: Scan UI + Progress
> Created: 2025-09-13

## Overview

Add a UI to trigger a library scan and display live progress and final totals. Provide backend endpoints to start a scan and report status, using the existing LibraryScanService under the hood. The UI must be responsive and usable on phones and tablets.

## User Stories

### Start a library scan from the UI
As a user, I want a "Scan Now" button so I can start indexing my music library when I first set up the app or after changing the library path.

Details: Clicking the button calls the scan endpoint; while running, the button is disabled and shows progress.

### See live scan progress and final counts
As a user, I want to see counts of total files processed, indexed tracks, and failures while the scan runs, and a final summary when it completes.

Details: The UI polls a status endpoint periodically to update progress; when complete, it shows the final totals and the time finished.

## Spec Scope

1. **Endpoints**
   - POST `/api/library/scan` — initiate a scan if not already running; returns 202 Accepted with a run id or simple status.
   - GET `/api/library/status` — return current status: `idle|running|completed`, counts `{ total, indexed, failed }`, and timestamps `{ startedAt, finishedAt? }`.
   - Concurrency: Return `409 Conflict` if a scan is already running and another start is requested.
2. **Frontend UI**
   - Add a visible "Scan Now" button.
   - While running: disable the button and show a compact progress indicator with counts.
   - When finished: show a success message with final counts; re-enable the button.
   - Poll status at ~750ms intervals while running (SSE/WebSocket optional future enhancement).
3. **Progress Tracking**
   - In-memory progress manager that coordinates a background scan task and exposes status.
   - Track `{ total, indexed, failed }`, current `status`, and timestamps.
4. **Mobile UX**
   - Phone-first layout with a single column; tablet can show two columns if desired.
   - Accessible progress region (aria-live polite), tap targets ≥44×44px.

## Out of Scope

- Persisting historical scan runs or logs beyond the current status.
- Frontend SSE/WebSocket streaming (polling only for this iteration).
- Progress UI beyond counts (e.g., per-folder view, ETA).

## Expected Deliverable

1. UI shows a "Scan Now" button; on click, scanning starts and live counts are shown; on completion, a summary is displayed and the button re-enables.
2. Backend provides POST `/api/library/scan` and GET `/api/library/status` with proper concurrency handling and 202/409/200 responses as applicable.
3. Mobile-friendly presentation with accessible status updates and minimal layout shifts.

