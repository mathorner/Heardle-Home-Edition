# Spec Tasks

- Spec: Scan UI + Progress
- Scope: Backend scan start/status endpoints with in-memory progress tracking; frontend UI to trigger scan and show live counts with mobile-friendly, accessible presentation.

## Tasks

- [x] 1. Backend progress manager + endpoints
  - [x] 1.1 Write tests for POST `/library/scan` (202 when started, 409 when already running; 400 when missing libraryPath) and GET `/library/status` (idle/running/completed)
  - [x] 1.2 Implement `ScanManager` singleton (thread-safe). State: `{ status, total, indexed, failed, startedAt?, finishedAt? }`; methods: `TryStart(Func<CancellationToken,Task<(int total,int indexed,int failed)>>)`, `GetStatus()`
  - [x] 1.3 Add endpoints: POST `/library/scan` initiates background task via `ILibraryScanService` guarded by `ScanManager`; GET `/library/status` returns manager snapshot
  - [x] 1.4 Wire DI and map endpoints; ensure 202/409/200 responses and proper timestamps
  - [x] 1.5 Verify all tests pass

- [x] 2. Frontend scan client and UI
  - [x] 2.1 Write tests for scan client (`startScan`, `getScanStatus`) and UI behavior (button disabled while running; progress text updates; completion summary)
  - [x] 2.2 Add `scanClient.ts` with `startScan()` → POST `/api/library/scan`, `getScanStatus()` → GET `/api/library/status`
  - [x] 2.3 Implement `ScanPanel` component with "Scan Now" button, aria-live progress region, and counts `{ total, indexed, failed }`
  - [x] 2.4 Integrate `ScanPanel` into the app (link from Settings/Home) and ensure mobile-friendly layout
  - [x] 2.5 Verify all tests pass

- [x] 3. Integration and resilience
  - [x] 3.1 Handle HTTP 409 gracefully in UI (show "Scan already running"; continue polling)
  - [x] 3.2 Poll at ~750ms while running; cancel polling on unmount; ignore AbortError; stop on completed
  - [x] 3.3 Verify all tests pass

- [x] 4. Docs & housekeeping
  - [x] 4.1 Update README with scan usage and endpoints
  - [x] 4.2 Update Product Plan to tick Iteration 4 when done
  - [x] 4.3 Verify all tests pass
