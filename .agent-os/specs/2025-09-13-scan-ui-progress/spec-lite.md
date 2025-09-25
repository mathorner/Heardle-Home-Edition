# Spec Summary (Lite)

Add a “Scan Now” button and a live progress display that polls the backend. Backend exposes POST `/api/library/scan` to start scanning and GET `/api/library/status` to report `{ status, total, indexed, failed, startedAt, finishedAt? }`. Prevent concurrent scans and provide a mobile-friendly UI.

