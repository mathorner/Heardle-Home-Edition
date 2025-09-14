# Spec Summary (Lite)

Implement a backend library scanner that recursively indexes MP3 files from the saved `libraryPath`, extracts `title` and `artist` (TagLib# with filename fallback), and writes `api/data/library.json` with `[ { id, title, artist, path } ]` using atomic write. Must handle large libraries, skip errors, and log a summary.

