# Spec Summary (Lite)

Expose range-enabled audio streaming for indexed tracks so the Play view can load `/api/audio/{trackId}` and resume playback across refreshes. Game session responses now return a safe `trackId`/`audioUrl`, the backend enforces graceful errors when files are missing, and the frontend renders an `<audio>` player wired to the new endpoint with updated tests and docs.

