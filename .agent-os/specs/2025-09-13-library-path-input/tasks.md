# Spec Tasks

- Spec: Library Path Input
- Scope: Settings UI + backend endpoint to validate and persist a user-provided absolute library path to `config/settings.json` with clear success/error handling.

## Tasks

- [ ] 1. Backend endpoint: validate, normalize, persist
  - [x] 1.1 Write tests for POST `/settings/library-path` (via proxy `/api/settings/library-path`)
  - [x] 1.2 Implement request DTO + validation (absolute path, exists, readable)
  - [x] 1.3 Implement readability check and error mapping to codes (`InvalidPath|NotFound|AccessDenied|Unknown`)
  - [x] 1.4 Ensure `config/` exists; atomic write to `config/settings.json`
  - [x] 1.5 Add minimal logging (redact sensitive details)
  - [x] 1.6 Verify all tests pass

- [ ] 2. Frontend settings UI and client
  - [x] 2.1 Write tests for Settings form (submit success shows confirmation; failure shows alert)
  - [x] 2.2 Add API client `saveLibraryPath(path: string)` posting to `/api/settings/library-path`
  - [x] 2.3 Implement Settings component (text input + Save, disable while submitting)
  - [x] 2.4 Wire Settings into the app (link or route) and basic styles
  - [x] 2.5 Verify all tests pass

- [ ] 3. Dev wiring and docs
  - [x] 3.1 Ignore `api/config/settings.json` in VCS; add placeholder (`.gitkeep`) to keep folder
  - [x] 3.2 Update README with usage (enter path), examples, and proxy note
  - [x] 3.3 Verify all tests pass
