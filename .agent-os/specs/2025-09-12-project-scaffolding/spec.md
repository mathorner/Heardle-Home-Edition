# Spec Requirements Document

> Spec: Project Scaffolding
> Created: 2025-09-12

## Overview

Establish runnable backend and frontend scaffolding with a minimal health check and mobile-first baseline, enabling rapid iteration on core gameplay. The frontend should load and display backend health status, with responsive layout suitable for phones and tablets.

## User Stories

### Developer can run and integrate

As a developer, I want a minimal .NET API and a React+TypeScript app running locally that can communicate, so that I can start implementing features quickly with a working baseline.

Details: Provide start scripts for backend and frontend, enable CORS for local dev, and confirm the frontend fetches the backend `/health` endpoint and renders its status.

### Mobile user sees a working home page

As a mobile user, I want the home page to render correctly and show the app’s health status, so that I know the app is working on my phone and tablet.

Details: Include proper mobile viewport meta, base responsive layout, accessible text/contrast, and touch-friendly sizing for initial controls.

## Spec Scope

1. **Backend Minimal API** - .NET 9 minimal API exposes `GET /health` returning 200 and JSON status (e.g., `{ "status": "ok" }`).
2. **Frontend Scaffold** - Vite React TypeScript app with a `/` page that displays app title and the fetched `/health` status.
3. **CORS for Dev** - Configure CORS to allow the local frontend origin during development.
4. **Mobile Baseline** - Add mobile viewport meta and a base responsive layout with design tokens (colors, spacing, typography scale).

## Out of Scope

- Library scanning, metadata extraction, or persistence.
- Audio streaming/snippets, game sessions, guessing logic, or FFmpeg.
- Production build/deploy packaging beyond stock tool defaults.

## Expected Deliverable

1. Frontend loads at its dev URL and renders a basic home page that fetches and displays `/health` from the backend.
2. Layout adapts cleanly at typical phone (320–414) and tablet (768–1024) widths; viewport meta present.
3. CORS permits frontend→backend requests in local development without errors.

