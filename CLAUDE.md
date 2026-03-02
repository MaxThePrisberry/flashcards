# Project Guidelines

## Environment

- .NET SDK is **not** installed locally. All backend build/test commands must use Docker.
- Node.js/npm are available locally for the frontend.

## Running Backend Tests

```bash
docker compose -f docker-compose.test.yml run --rm test
```

This uses `Dockerfile.test` and `docker-compose.test.yml` to build and run `backend.Tests/` (xUnit) against a disposable Postgres instance.

## Key Files

- `docs/api-contracts.md` — API contract (source of truth for all endpoints)
- `backend/` — ASP.NET Core 8 backend
- `backend.Tests/` — xUnit integration tests
- `frontend/` — Next.js 15 + TypeScript frontend (see `frontend/CLAUDE.md` for frontend-specific rules)
- `docker-compose.yml` — development services (postgres, backend, frontend)
- `docker-compose.test.yml` — test services (test-db, test runner)
- `Dockerfile.test` — test Docker image (uses .NET 8 SDK)
