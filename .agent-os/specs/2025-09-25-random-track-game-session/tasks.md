# Spec Tasks

- [x] 1. Library index provider and cache
  - [x] 1.1 Write tests for library index provider (load success, missing file → LibraryNotReady, empty tracks → NoTracksIndexed, cache refresh on timestamp change, skip missing file paths)
  - [x] 1.2 Implement `ILibraryIndexProvider` with caching, timestamp detection, and error codes
  - [x] 1.3 Wire provider into DI and invalidate cache from `LibraryScanService` after writes
  - [x] 1.4 Verify all tests pass

- [x] 2. Game session store
  - [x] 2.1 Write tests for in-memory session store creation, retrieval, mutation hook, and expiry sweep
  - [x] 2.2 Implement `IGameSessionStore` with thread-safe storage and expiration handling
  - [x] 2.3 Add logging hooks for creation and cleanup events
  - [x] 2.4 Verify all tests pass

- [x] 3. Game API endpoints
  - [x] 3.1 Write integration tests for POST `/game/start` covering success, LibraryNotReady (503), and NoTracksIndexed (409)
  - [x] 3.2 Write tests for GET `/game/{id}` covering success, invalid GUID (400), and missing session (404)
  - [x] 3.3 Implement endpoint handlers, DTOs, and wiring in `Program` with structured error responses and `Location` header
  - [x] 3.4 Verify all tests pass

- [x] 4. Frontend game client and Play view
  - [x] 4.1 Write unit tests for `gameClient` (start success/errors, get session success/404)
  - [x] 4.2 Write component tests for `Game` view (start flow, resume existing game, error handling, storage clearing)
  - [x] 4.3 Implement `gameClient.ts`, new `Game` component, navigation updates, and sessionStorage integration
  - [x] 4.4 Ensure mobile-first layout, accessibility messaging, and actionable error prompts
  - [x] 4.5 Verify all tests pass

- [ ] 5. Integration polish and documentation
  - [ ] 5.1 Plumb provider invalidation after scan completion and add any necessary telemetry/logging tweaks
  - [ ] 5.2 Update README and Product Plan with Iteration 5 status and new endpoints/flow
  - [ ] 5.3 Run full test suites (`dotnet test`, `npm test -- --run`)
  - [ ] 5.4 Verify all tests pass
