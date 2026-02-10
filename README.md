# Flashcards

A full-stack flashcard application built with Next.js, ASP.NET Core, and PostgreSQL.

## Prerequisites

### macOS

1. Install [Docker Desktop for Mac](https://www.docker.com/products/docker-desktop/):
   - Download the `.dmg` for your chip (Apple Silicon or Intel)
   - Drag Docker to Applications and launch it
   - Docker Compose is included automatically

2. Verify the installation:
   ```bash
   docker --version
   docker compose version
   ```

### Linux (Ubuntu/Debian)

1. Install Docker and the Compose plugin:
   ```bash
   sudo apt update
   sudo apt install docker.io docker-compose-v2
   ```

2. Allow your user to run Docker without `sudo`:
   ```bash
   sudo usermod -aG docker $USER
   ```
   Log out and back in for this to take effect.

3. Verify the installation:
   ```bash
   docker --version
   docker compose version
   ```

## Quick Start

1. Clone the repository:
   ```bash
   git clone <repo-url>
   cd flashcards
   ```

2. Start all services:
   ```bash
   docker compose up --build
   ```

3. Verify everything is running:
   ```bash
   curl http://localhost:5000/health
   ```
   You should see `{"status":"healthy"}`. Then open http://localhost:3000 in your browser.

To stop the services, press `Ctrl+C` or run `docker compose down`.

| Service  | URL                          |
|----------|------------------------------|
| Frontend | http://localhost:3000         |
| Backend  | http://localhost:5000/health  |
| Postgres | localhost:5432               |

## Project Structure

```
frontend/   → Next.js 15 + TypeScript
backend/    → ASP.NET Core 8 Minimal API
```

## Database

PostgreSQL 16 with default credentials for local development:

- **User:** flashcards
- **Password:** flashcards
- **Database:** flashcards
