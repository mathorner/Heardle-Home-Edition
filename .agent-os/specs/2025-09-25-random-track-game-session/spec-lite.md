# Spec Summary (Lite)

Add backend game session management that reads the indexed library, picks a random track, and exposes POST `/api/game/start` plus GET `/api/game/{id}`. Store sessions in memory with attempt/status metadata, return structured errors when the library is not ready, and build a "Play" UI that starts/re-hydrates rounds with clear guidance on scanning first.

