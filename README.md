# Flashcards

A full-stack flashcard application built with Next.js, ASP.NET Core, and PostgreSQL.

## Quick Start

```bash
docker compose up --build
```

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
