# Heardle Home Edition - Product Plan

## Executive Summary

Heardle Home Edition is a music guessing game that uses the player's personal music library instead of a curated playlist. Players listen to progressively longer snippets of songs from their own MP3 collection and attempt to guess the title and artist within 6 attempts.

## Product Vision

**Mission**: Create an engaging, personalized music guessing experience that transforms your personal music library into an interactive game.

**Vision**: A seamless, mobile-friendly web application that brings the joy of music discovery to your own collection, making every song a potential puzzle to solve.

## Target Audience

- **Primary**: Music enthusiasts with large personal MP3 collections
- **Secondary**: Families and friends looking for interactive music-based entertainment
- **Tertiary**: Users who enjoy puzzle/guessing games with a musical twist

## Core Value Propositions

1. **Personalization**: Uses your own music library, making every game unique to you
2. **Accessibility**: No subscription required, works with existing MP3 collections
3. **Progressive Challenge**: Increasing snippet lengths create escalating difficulty
4. **Mobile-First**: Optimized for phones and tablets for on-the-go gaming
5. **Offline Capable**: Works with local network shares, no internet required for gameplay

## Product Requirements

### Functional Requirements

#### Core Gameplay
- **Library Integration**: Scan and index MP3 files from network/local folders
- **Random Selection**: Algorithmically select random tracks for each game
- **Progressive Snippets**: Play snippets of increasing length (1, 2, 4, 7, 11, 16 seconds)
- **Guess Validation**: Strict matching of title and artist (case-insensitive, punctuation-agnostic)
- **Attempt Management**: Maximum 6 attempts per game
- **Reveal System**: Show correct answer after failed attempts

#### Technical Requirements
- **Backend**: .NET 9 with ASP.NET Core Minimal APIs
- **Frontend**: React + TypeScript with Vite
- **Audio Processing**: FFmpeg integration for precise snippet generation
- **File Access**: Network share support for distributed music libraries
- **Mobile Support**: Responsive design with touch-optimized controls

### Non-Functional Requirements

#### Performance
- **Audio Streaming**: Support for range requests and smooth playback
- **Library Scanning**: Handle large collections (10,000+ tracks) efficiently
- **Mobile Performance**: Optimized for iOS Safari and Android Chrome
- **Network Efficiency**: Minimal bandwidth usage for audio streaming

#### Usability
- **Mobile-First Design**: Touch-friendly interface for phones and tablets
- **Responsive Layout**: Mobile-first design targeting 320–414 (phones) and 768–1024 (tablets) widths
- **Touch Targets**: Minimum 44×44px interactive elements; avoid double‑tap zoom (`touch-action: manipulation`)
- **Safe Areas**: Respect iOS notches using CSS env(safe-area-inset-*); sticky bottom controls on small screens
- **Viewport/Meta**: `meta viewport` configured; `playsInline` on `<audio>` to prevent fullscreen on iOS
- **Audio Gestures**: First playback must be user‑initiated on iOS; provide a "Tap to enable sound" gate if needed
- **Performance**: Lightweight UI, lazy render where possible; avoid layout thrash; prefer CSS over JS animations
- **Accessibility**: High contrast theme, focus styles, semantic controls; large text friendly

#### Reliability
- **File System Resilience**: Handle missing, locked, or corrupted audio files
- **Network Share Support**: Robust handling of UNC paths and credentials
- **Cross-Platform**: Consistent experience across desktop and mobile browsers

## Technical Architecture

### System Components

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   React SPA     │    │  .NET 9 API     │    │  File System    │
│   (Frontend)    │◄──►│   (Backend)     │◄──►│  (MP3 Library)  │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│  HTML5 Audio    │    │   FFmpeg        │    │  Network Share  │
│  (Playback)     │    │  (Snippets)     │    │  (UNC Paths)    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### Technical Stack Details

- **Backend**: ASP.NET Core (.NET 9), Minimal APIs, C#
- **Frontend**: React + TypeScript (Vite), HTML5 `<audio>`
- **Metadata**: TagLib# for ID3 tags (title, artist)
- **Index**: JSON on disk for simplicity (`data/library.json`), upgradeable to SQLite later
- **Audio**: Range-enabled full-track endpoint; server-cut exact-length snippets via FFmpeg
- **Sessions**: In-memory game sessions (single-user focus), GUID `gameId`
- **Mobile-first**: Responsive UI, touch targets, iOS/Android audio constraints considered

### Data Flow

1. **Library Setup**: User selects folder → Backend scans MP3s → Creates index
2. **Game Start**: Frontend requests new game → Backend selects random track
3. **Snippet Playback**: Frontend requests snippet → Backend generates via FFmpeg → Streams to client
4. **Guess Processing**: Frontend submits guess → Backend validates → Returns result
5. **Game End**: Backend reveals answer → Frontend displays result

### API Design

#### Core Endpoints
- `POST /api/settings/library-path` - Save/validate library path
- `POST /api/library/scan` - Start scan; returns a job id
- `GET /api/library/status` - Scan progress + track count
- `POST /api/game/start` - Create session, pick random track
- `GET /api/game/{id}/snippet?attempt=n` - Stream exact-length snippet for attempt n
- `POST /api/game/{id}/guess` - Validate a guess and update session
- `GET /api/audio/{trackId}` - Range-enabled full track stream (`audio/mpeg`)
- `POST /api/game/{id}/reveal` - Reveal title/artist; mark finished

## Implementation Roadmap

### Phase 1: MVP Foundation (Iterations 1-10)
**Goal**: Deliver core gameplay functionality

#### Iteration 1: Project Scaffolding
- **Goal**: Create runnable backend and frontend shells
- **Deliverables**: 
  - [x] .NET 9 Minimal API with `/health`
  - [x] Vite React TS app with `/` page
  - [x] CORS enabled
  - [x] Mobile viewport meta
  - [x] Base responsive layout and design tokens
- **Acceptance**: Both apps run locally; frontend can hit `/health` and display status; layout adapts cleanly at phone and tablet widths

#### Iteration 2: Settings: Library Path Input
- **Goal**: Let user enter a folder path; store it
- **Deliverables**: 
  - [x] POST `/api/settings/library-path`
  - [x] Server validates read access
  - [x] Persist to `config/settings.json`
- **Acceptance**: Valid path saves; invalid path returns clear error message

#### Iteration 3: Library Scan (Backend)
- **Goal**: Scan MP3s and extract metadata
- **Deliverables**: 
  - [x] Scan service
  - [x] TagLib# integration
  - [x] Write `data/library.json` with `[{ id, title, artist, path }]`
- **Acceptance**: Scans nested folders; handles large libraries; logs errors but continues

#### Iteration 4: Scan UI + Progress
- **Goal**: Trigger scan and view progress
- **Deliverables**: 
  - [x] POST `/api/library/scan`
  - [x] GET `/api/library/status`
  - [x] Frontend "Scan Now" button
  - [x] Progress indicator
  - [x] Final track count display
- **Acceptance**: User sees live progress updates and final totals; UI remains usable on phone (one-column) and tablet (two-column optional)

#### Iteration 5: Random Track + Game Session
- **Goal**: Start a game with a random pick
- **Deliverables**: 
  - [ ] POST `/api/game/start` returns `{ gameId, attempt: 1 }`
  - [ ] In-memory session storing `trackId`, attempts, and status (`active|won|lost`)
- **Acceptance**: Always returns a valid `gameId`; session is stored; no audio yet

#### Iteration 6: Audio Streaming (Full Track)
- **Goal**: Serve audio with range requests for full track playback
- **Deliverables**: 
  - [ ] GET `/api/audio/{trackId}` using `PhysicalFile` (enable range processing) with `audio/mpeg`
- **Acceptance**: HTML `<audio>` can play and scrub the selected track

#### Iteration 7: Snippet Orchestration (Frontend-Timed)
- **Goal**: Play controlled snippets per attempt from the start of the track
- **Deliverables**: 
  - [ ] Snippet schedule [1, 2, 4, 7, 11, 16]
  - [ ] Frontend timer pauses playback at target
  - [ ] Skip advances to next attempt
  - [ ] UI shows attempt number
  - [ ] "Tap to enable sound" overlay to unlock audio on iOS
  - [ ] Use `<audio playsInline>` and trigger initial `play()` within a user gesture
- **Acceptance**: Snippets play to within ~50ms; user can initiate audio on iOS/Android without fullscreen interruptions; no server snippet trimming yet

#### Iteration 8: Guess Input + Validation
- **Goal**: Allow guesses and validate strictly (title + artist)
- **Deliverables**: 
  - [ ] POST `/api/game/{id}/guess` with `{ text }`
  - [ ] Normalization strips punctuation, case-insensitive, trims/collapses whitespace
  - [ ] Exact match required
  - [ ] Response `{ correct, normalizedGuess, answer? }`
- **Acceptance**: Correct guess ends round immediately; incorrect advances to next attempt

#### Iteration 9: Round End + Reveal + Play Again
- **Goal**: Finish the round and handle reveal
- **Deliverables**: 
  - [ ] On success: show "Correct! [Title – Artist]" and button to play full track
  - [ ] On fail at attempt 6: reveal and show "Play Again"
  - [ ] POST `/api/game/{id}/reveal` to mark finished
- **Acceptance**: End-of-round logic consistent; "Play Again" starts a new random game

#### Iteration 10: Exact-Length Snippets (Server-Side)
- **Goal**: Guarantee exact snippet durations independent of client timing/VBR
- **Deliverables**: 
  - [ ] GET `/api/game/{id}/snippet?attempt=n` pipes FFmpeg (`-ss 0 -t {duration}s -i <path> -f mp3 -`) with `audio/mpeg`
  - [ ] Config `FFMPEG_PATH` if needed
  - [ ] Frontend uses snippet endpoint instead of client-timed playback
- **Acceptance**: Snippets are exact length (±10ms) for each attempt across desktop, iOS Safari, and Android Chrome

### Phase 2: Polish & Robustness (Iterations 11-16)
**Goal**: Production-ready experience

#### Iteration 11: Robustness & Edge Cases
- **Goal**: Make the happy path resilient
- **Deliverables**: 
  - [ ] Fallback when tags missing (infer from filename)
  - [ ] Handle missing/locked files
  - [ ] Clear errors for disconnected shares
  - [ ] Small retry for transient I/O
  - [ ] Skip unreadable files
- **Acceptance**: Failed files are skipped gracefully; UI surfaces friendly messages

#### Iteration 12: Persistence & Startup UX
- **Goal**: Smooth startup experience
- **Deliverables**: 
  - [ ] Persist last library path
  - [ ] Auto-detect missing index and prompt to scan
  - [ ] "Rescan" button
  - [ ] Basic stats (track count)
- **Acceptance**: Fresh start is guided; no dead ends

#### Iteration 13: Fuzzy Matching (Flagged)
- **Goal**: Make guessing less brittle (optional)
- **Deliverables**: 
  - [ ] Normalizer improvements (collapse whitespace, strip punctuation, remove common feat./remix annotations)
  - [ ] Optional fuzzy threshold (e.g., Levenshtein) behind a feature flag
- **Acceptance**: Near-miss handling can be tuned; default behavior remains strict

#### Iteration 14: Suggest-As-You-Type (Flagged)
- **Goal**: Help users recall titles quickly
- **Deliverables**: 
  - [ ] Client-side suggestions from the index
  - [ ] Keyboard navigation
  - [ ] Optional toggle
- **Acceptance**: Suggestions appear quickly (<50ms) for up to ~10k tracks

#### Iteration 15: Packaging & Deployment
- **Goal**: Make it easy to run at home
- **Deliverables**: 
  - [ ] Dockerfile
  - [ ] System service sample
  - [ ] Config docs for network shares (UNC paths, credentials)
  - [ ] Env vars (`LIBRARY_PATH`, `FFMPEG_PATH`)
- **Acceptance**: Single command/container deploy; network share setup documented

#### Iteration 16: Mobile Installability (PWA) & UX Polish
- **Goal**: Make the web app installable and polished on phones/tablets
- **Deliverables**: 
  - [ ] Web App Manifest (icons, name, theme color)
  - [ ] Basic Service Worker for app‑shell caching (exclude audio)
  - [ ] `display: standalone`
  - [ ] iOS meta tags and splash icons
  - [ ] Media Session API for lockscreen controls after reveal
  - [ ] Wake Lock during active round (with fallback/no‑sleep shim)
  - [ ] Safe‑area CSS polish
  - [ ] Orientation tested (portrait primary)
- **Acceptance**: App can be added to home screen on iOS/Android; opens full‑screen; doesn't sleep during a round; controls are touch‑friendly and accessible

## Data Models

### Core Data Structures
- **Track**: `{ id: string, title: string, artist: string, path: string }`
- **LibraryIndex**: `{ version: number, tracks: Track[] }`
- **GameSession**: `{ id: string, trackId: string, attempt: number, status: 'active'|'won'|'lost' }`

### Validation Rules
- **Normalization**: lowercase, remove punctuation, trim, collapse whitespace
- **Strict Match**: `norm(userTitle) == norm(actualTitle)` AND `norm(userArtist) == norm(actualArtist)`

### Audio Technical Notes
- **MVP Accuracy**: Achieved via server-side FFmpeg snippets (Iteration 10)
- **Frontend Timing**: Used earlier as a fallback (Iteration 7)
- **Range Support**: Required for full-track playback and smooth seeking
- **Mobile Specifics**: First audio must be user‑initiated (iOS); use `playsInline`; avoid autoplay; prefetch metadata only; ensure correct `Accept-Ranges`/`Content-Range` headers for scrubbing

## Development Setup

### Prerequisites
- .NET 9 SDK
- Node 20+
- FFmpeg installed and on PATH

### Environment Variables
- `LIBRARY_PATH` - Path to music library folder
- `FFMPEG_PATH` - Path to FFmpeg executable (optional early; required by Iteration 10)

### Network Share Configuration
- Ensure backend host can access the library path with read permissions
- Support for UNC paths and credentials
- LAN testing: Access the frontend from a phone/tablet via `http://<host-ip>:<port>` on the same Wi‑Fi; verify audio unlock flow on iOS Safari and Android Chrome

## Success Metrics

### Primary Metrics
- **Game Completion Rate**: Percentage of games completed (not abandoned)
- **Guess Accuracy**: Average attempts needed to guess correctly
- **Library Utilization**: Percentage of library tracks that have been played
- **Session Duration**: Average time spent per gaming session

### Technical Metrics
- **Audio Load Time**: Time from snippet request to playback start
- **Library Scan Performance**: Tracks scanned per second
- **Mobile Compatibility**: Successful playback on iOS/Android devices
- **Error Rate**: Percentage of failed audio requests or game sessions

### User Experience Metrics
- **Mobile Usage**: Percentage of sessions on mobile devices
- **Return Rate**: Users who play multiple games in a session
- **Feature Adoption**: Usage of advanced features (fuzzy matching, suggestions)

## Risk Assessment

### Technical Risks
- **Audio Compatibility**: Different MP3 encodings may cause playback issues
  - *Mitigation*: Comprehensive testing with various file formats
- **Network Share Access**: Complex authentication and permission issues
  - *Mitigation*: Clear documentation and fallback to local paths
- **Mobile Audio Limitations**: iOS/Android audio policy restrictions
  - *Mitigation*: Proper user gesture handling and audio unlock flow

### Product Risks
- **Library Size**: Very large collections may impact performance
  - *Mitigation*: Efficient indexing and lazy loading strategies
- **User Adoption**: Limited appeal to users without large music collections
  - *Mitigation*: Focus on music enthusiasts and provide sample libraries
- **Maintenance**: Ongoing support for various file formats and network configurations
  - *Mitigation*: Robust error handling and user-friendly error messages

## Competitive Analysis

### Direct Competitors
- **Original Heardle**: Uses curated playlists, limited to specific songs
- **Spotify Heardle**: Requires Spotify subscription, limited song selection

### Competitive Advantages
- **Personal Library**: Uses user's own music, unlimited variety
- **No Subscription**: Free to use with existing music collection
- **Offline Capable**: Works without internet connection
- **Customizable**: User controls the entire music selection

## Future Opportunities

### Short-term Enhancements
- **Multiplayer Mode**: Compete with friends using the same library
- **Difficulty Settings**: Adjustable snippet lengths and attempt counts
- **Statistics Tracking**: Personal bests and library exploration metrics
- **Playlist Integration**: Support for specific playlist-based games

### Long-term Vision
- **AI-Powered Features**: Smart difficulty adjustment based on user performance
- **Social Features**: Share games and compete with friends
- **Music Discovery**: Suggest similar artists based on guessing patterns
- **Cross-Platform**: Native mobile apps with enhanced features

## Conclusion

Heardle Home Edition represents a unique opportunity to create a personalized music gaming experience that leverages users' existing music collections. The iterative development approach ensures rapid delivery of core functionality while maintaining flexibility for future enhancements. The mobile-first design and robust technical architecture position the product for success across multiple platforms and use cases.

The key to success will be delivering a smooth, reliable experience that makes users want to return to their music libraries in a new, interactive way. By focusing on the MVP first and then iterating based on user feedback, we can build a product that truly resonates with music lovers and gaming enthusiasts alike.
