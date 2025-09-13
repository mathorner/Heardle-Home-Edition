# Spec Requirements Document

> Spec: Library Path Input
> Created: 2025-09-13

## Overview

Add a simple settings flow that lets the user enter a music library folder path, validates that the path is accessible on the server, and persists it for later iterations. Provide clear success and error feedback in the UI.

## User Stories

### Enter and save a valid library path

As a user, I want to enter the path to my music folder and save it, so that the app knows where to find my MP3s in later steps.

Details: The server validates the path (absolute path, exists, readable) and persists it to `config/settings.json`. The UI shows a success confirmation.

### See a clear error for invalid paths

As a user, I want a clear error if the path is invalid or not readable, so that I can correct it quickly.

Details: The server returns a structured error (with a reason) and the UI displays an actionable message.

## Spec Scope

1. **Settings UI** - A minimal UI control (single input + Save) to enter the library folder path and see success/error states.
2. **POST /api/settings/library-path** - Accept `{ path }`, validate (absolute, exists, readable), and persist on success.
3. **Persist to config** - Save to `config/settings.json` as `{ "libraryPath": "<absolute path>" }`; create folder if missing; atomic write.
4. **Validation and normalization** - Normalize path, require absolute path, and return specific error codes/messages on failure.

## Out of Scope

- Scanning the library, showing track counts, or reading any files yet.
- Credentials prompts for network shares; mounting remote drives.
- Listing/browsing directories or autocompleting paths.
- Any additional settings beyond `libraryPath`.

## Expected Deliverable

1. Submitting a valid readable absolute path returns 200 and persists it to `config/settings.json`.
2. Submitting an invalid/unreadable path returns a 400 response with a clear error code and message; UI shows an error state.
3. The UI form allows entering and saving the path and shows a success confirmation for valid paths.

