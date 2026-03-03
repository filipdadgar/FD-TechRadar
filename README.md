# IoT & Connectivity Tech Radar

A self-hosted tech radar focused on the IoT and connectivity domain. An agent worker automatically discovers relevant technologies from RSS feeds and GitHub, queues proposals for admin review, and publishes accepted entries to the public radar.

---

## Architecture

```
                        ┌─────────────────────────────────────┐
                        │              nginx :80               │
                        │  /api/*  →  api:8080                │
                        │  /admin/ →  frontend-admin:80       │
                        │  /*      →  frontend-web:80         │
                        └─────────────────────────────────────┘
                               │              │           │
                        ┌──────┘       ┌──────┘    ┌──────┘
                        ▼             ▼            ▼
                   ┌─────────┐  ┌──────────┐  ┌──────────────┐
                   │   API   │  │  Admin   │  │  Public Web  │
                   │ ASP.NET │  │  React   │  │    React     │
                   │  :8080  │  │   SPA    │  │     SPA      │
                   └────┬────┘  └──────────┘  └──────────────┘
                        │
               ┌────────┴────────┐
               │                 │
               ▼                 ▼
        ┌────────────┐   ┌───────────────┐
        │ PostgreSQL │   │   Workers     │
        │     16     │   │  .NET BgSvc   │
        │            │   │  (RSS/GitHub) │
        └────────────┘   └───────────────┘
```

**Six Docker services:**

| Service | Image | Role |
|---------|-------|------|
| `postgres` | `postgres:16-alpine` | Primary data store |
| `api` | Built from `Dockerfile.api` | REST API + JWT auth + EF migrations on startup |
| `workers` | Built from `Dockerfile.workers` | Scheduled/manual agent scans |
| `frontend-web` | Built from `Dockerfile.web` | Public radar viewer (React) |
| `frontend-admin` | Built from `Dockerfile.admin` | Admin panel (React) |
| `nginx` | `nginx:alpine` | Reverse proxy on port 80 |

---

## Repository Layout

```
.
├── backend/
│   ├── TechRadar.slnx                  # .NET solution file
│   ├── src/
│   │   ├── TechRadar.Api/              # ASP.NET Core 10 REST API
│   │   ├── TechRadar.Core/             # Domain models, interfaces, services
│   │   ├── TechRadar.Data/             # EF Core, repositories, migrations
│   │   └── TechRadar.Workers/          # BackgroundService agent workers
│   └── tests/
│       ├── TechRadar.Api.Tests/
│       └── TechRadar.Workers.Tests/
│
├── frontend/
│   ├── web/                            # Public radar viewer (Vite + React + TS)
│   └── admin/                          # Admin management panel (Vite + React + TS)
│
└── docker/
    ├── docker-compose.yml              # Production compose file
    ├── docker-compose.dev.yml          # Dev overrides (exposed ports)
    ├── .env.example                    # Environment variable template
    ├── Dockerfile.api
    ├── Dockerfile.workers
    ├── Dockerfile.web
    ├── Dockerfile.admin
    └── nginx/
        ├── nginx.conf                  # Reverse proxy routing
        └── spa.conf                    # SPA nginx config (shared by both frontends)
```

---

## Prerequisites

- **Docker Engine 24+** and **Docker Compose v2+**
- **Git**

For local development without Docker:
- **.NET 10 SDK**
- **Node.js 20+**
- **PostgreSQL 16** (or use the compose `postgres` service)

---

## Quick Start

### 1. Clone

```bash
git clone <repo-url>
cd techradar-agentic
```

### 2. Configure

```bash
cp docker/.env.example docker/.env
```

Edit `docker/.env` — the four required values:

```env
ADMIN_USERNAME=admin
ADMIN_PASSWORD=<strong password>
JWT_SECRET=<run: openssl rand -base64 48>
POSTGRES_PASSWORD=<database password>
```

Optional values (leave commented out to use defaults):

```env
# GitHub PAT: improves agent rate limits from 10 to 30 req/min
# GITHUB_PAT=ghp_...

# Anthropic API key: enables LLM-enriched proposals.
# Without this the system runs in manual-classification mode —
# agents still discover technologies, but quadrant/ring must be set by the admin.
# ANTHROPIC_API_KEY=sk-ant-...
```

### 3. Start

```bash
cd docker
docker compose up -d
```

Wait ~15 seconds for the API to run database migrations, then verify:

```bash
docker compose ps
# All services: Up (healthy)

curl http://localhost/api/healthz
# {"status":"healthy","db":"connected"}
```

### 4. Open

| URL | What you get |
|-----|-------------|
| `http://localhost` | Public radar viewer |
| `http://localhost/admin` | Admin panel (login with your `.env` credentials) |

---

## Environment Variables

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `ADMIN_USERNAME` | Yes | — | Admin login username |
| `ADMIN_PASSWORD` | Yes | — | Admin login password |
| `JWT_SECRET` | Yes | — | JWT signing secret (32+ chars); generate with `openssl rand -base64 48` |
| `POSTGRES_PASSWORD` | Yes | — | PostgreSQL password |
| `GITHUB_PAT` | No | — | GitHub Personal Access Token (no scopes needed for public search) |
| `ANTHROPIC_API_KEY` | No | — | Enables LLM classification of agent proposals |
| `AGENTS_SCHEDULE_CRON` | No | `0 3 * * *` | Cron expression for scheduled agent runs (UTC) |
| `AGENTS_MAX_PROPOSALS_PER_RUN` | No | `50` | Max proposals generated per agent run |
| `AGENTS_STALE_PROPOSAL_THRESHOLD_DAYS` | No | `7` | Days before a pending proposal is flagged stale |
| `AGENTS_ANTHROPIC_MODEL` | No | `claude-haiku-4-5-20251001` | Anthropic model for classification |

---

## Backend

**Stack:** C# / .NET 10, ASP.NET Core 10, Entity Framework Core 10, PostgreSQL 16 (Npgsql)

### Projects

| Project | Purpose |
|---------|---------|
| `TechRadar.Core` | Domain models, repository interfaces, application services, `ILlmClassifier` |
| `TechRadar.Data` | `TechRadarDbContext`, EF Core configurations, migrations, repository implementations |
| `TechRadar.Api` | REST API controllers, JWT auth middleware, DI wiring, auto-migrate on startup |
| `TechRadar.Workers` | `AgentScanWorker` (BackgroundService), RSS + GitHub collectors, `AgentScanService` |

### Build & Run locally

```bash
# From repo root
dotnet build backend/TechRadar.slnx

# Run the API (requires a running PostgreSQL — use docker compose for the DB only)
cd backend/src/TechRadar.Api
POSTGRES_HOST=localhost POSTGRES_PORT=5432 POSTGRES_DB=techradar \
  POSTGRES_USER=techradar POSTGRES_PASSWORD=techradar_dev \
  ADMIN_USERNAME=admin ADMIN_PASSWORD=admin \
  JWT_SECRET=dev_secret_at_least_32_chars_long \
  dotnet run
```

### EF Core migrations

```bash
cd backend/src/TechRadar.Data

# Add a new migration
dotnet ef migrations add <MigrationName> \
  --startup-project ../TechRadar.Api \
  --project .

# Apply to a running database
dotnet ef database update \
  --startup-project ../TechRadar.Api \
  --project .
```

### API Overview

All routes are prefixed with `/api` at the nginx layer.

**Public (no auth):**

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/radar/current` | Current radar state grouped by quadrant |
| `GET` | `/radar/entries` | All active entries (filterable by quadrant, ring, tag) |
| `GET` | `/radar/entries/{id}` | Single entry with ring change history |
| `GET` | `/radar/snapshots` | List of historical radar snapshots |
| `GET` | `/radar/snapshots/{id}` | Single snapshot |
| `GET` | `/radar/snapshots/compare` | Diff between two snapshots (`?fromId=&toId=`) |
| `GET` | `/healthz` | Health check |

**Admin (Bearer JWT required):**

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/admin/auth/login` | Get JWT token |
| `GET/POST` | `/admin/entries` | List / create technology entries |
| `GET/PUT/DELETE` | `/admin/entries/{id}` | Read / update / archive entry |
| `GET` | `/admin/proposals` | List proposals (filterable by status) |
| `POST` | `/admin/proposals/{id}/accept` | Accept (with optional overrides) |
| `POST` | `/admin/proposals/{id}/reject` | Reject with optional reason |
| `GET` | `/admin/agent-runs` | Agent run history |
| `POST` | `/admin/agents/trigger` | Trigger an immediate agent scan |
| `GET/POST` | `/admin/sources` | List / create data sources |
| `GET/PUT/DELETE` | `/admin/sources/{id}` | Read / update / delete source |

### LLM mode vs manual mode

| Condition | Behaviour |
|-----------|-----------|
| `ANTHROPIC_API_KEY` set | `AnthropicLlmClassifier` — proposals include `recommendedQuadrant`, `recommendedRing`, `evidenceSummary` |
| `ANTHROPIC_API_KEY` absent | `PassthroughClassifier` — proposals queued without classification; admin must set quadrant/ring before accepting |

---

## Frontend — Web (Public Radar Viewer)

**Stack:** React 18, TypeScript, Vite

**Served at:** `http://localhost/`

**Dev server:**

```bash
cd frontend/web
npm install
npm run dev      # → http://localhost:5173
```

`/api` is proxied to `http://localhost:5000` in dev mode (see `vite.config.ts`).

### Views

- **Radar View** — SVG circular chart with concentric rings (Adopt → Hold), four quadrant sectors, coloured dots per entry. Click any dot to open the detail panel.
- **List View** — Filterable table with quadrant and ring dropdowns.
- **Detail panel** — Slides in on entry click; shows description, rationale, tags, and full ring change history.

---

## Frontend — Admin Panel

**Stack:** React 18, TypeScript, Vite

**Served at:** `http://localhost/admin/`

**Dev server:**

```bash
cd frontend/admin
npm install
npm run dev      # → http://localhost:5174
```

`/api` is proxied to `http://localhost:5000` in dev mode.

### Pages

| Page | Path | Description |
|------|------|-------------|
| Login | `/admin` | JWT login form |
| Entries | Entries tab | Create, edit, move ring, archive technologies |
| Proposals | Proposals tab | Review agent proposals; accept / edit / reject |
| Agents | Agents tab | View run history; trigger manual scan |
| Sources | Sources tab | Manage RSS feeds and GitHub topic sources |
| History | History tab | Browse and compare radar snapshots |

---

## Docker

All Dockerfiles are in `docker/` and use `..` (repo root) as the build context.

```bash
# From docker/ directory

# Start all services
docker compose up -d

# Start with dev overrides (exposes API on :5000, postgres on :5432)
docker compose -f docker-compose.yml -f docker-compose.dev.yml up -d

# View logs
docker compose logs -f api
docker compose logs -f workers

# Stop (keeps data volume)
docker compose down

# Stop and destroy all data
docker compose down -v

# Rebuild a single service
docker compose build api
docker compose up -d --no-deps api
```

### Use the published image (optional)

If you just want to try the API quickly you can pull the image published to GitHub Packages (GitHub Container Registry).

Replace `<OWNER/REPO>` and `<TAG>` with your repository and tag (for this repo `filipdadgar/FD-TechRadar` and e.g. `main`):

```bash
# pull the published image
docker pull ghcr.io/<OWNER/REPO>:<TAG>

# run it (maps container port 80 → host 5000)
docker run --rm -p 5000:80 \
  -e POSTGRES_HOST=host.docker.internal -e POSTGRES_PORT=5432 \
  -e POSTGRES_DB=techradar -e POSTGRES_USER=techradar -e POSTGRES_PASSWORD=techradar \
  ghcr.io/<OWNER/REPO>:<TAG>

# then visit http://localhost:5000/api/healthz
```

Notes:
- The image published by the CI is the `api` image built from `docker/Dockerfile.api`.
- The container listens on the standard HTTP port (80) by default; we map it to `5000` above to avoid conflicts with a local nginx.

### Build and run locally (single service)

If you prefer to build the API image locally instead of pulling the published package:

```bash
# from repo root
docker build -f docker/Dockerfile.api -t fd-techradar-api:local .

# run it (map to host port 5000)
docker run --rm -p 5000:80 \
  -e POSTGRES_HOST=host.docker.internal -e POSTGRES_PORT=5432 \
  -e POSTGRES_DB=techradar -e POSTGRES_USER=techradar -e POSTGRES_PASSWORD=techradar \
  fd-techradar-api:local
```

If you want the exact same image name as the published package, tag it and push to your registry:

```bash
docker tag fd-techradar-api:local ghcr.io/<OWNER/REPO>:local
docker push ghcr.io/<OWNER/REPO>:local
```

### Multiple images / CI behaviour

Currently the GitHub workflow builds and publishes a single image (the API) using `docker/Dockerfile.api` — that's why you saw only one package in GitHub Packages. This is by design in the current workflow. If you'd like, I can update the workflow to build a matrix of images (API, workers, web, admin) or add separate `build-push` steps for each Dockerfile so all services are published. Which images would you like published from CI?

### Port mapping (production)

| Host port | Service |
|-----------|---------|
| `80` | nginx — all traffic routed here |

### Port mapping (dev overrides)

| Host port | Service |
|-----------|---------|
| `80` | nginx |
| `5000` | api (direct) |
| `5432` | postgres (direct) |

---

## Agentic Scan Pipeline

The `workers` service runs `AgentScanWorker`, a .NET `BackgroundService` that:

1. Wakes on a cron schedule (`AGENTS_SCHEDULE_CRON`) or on a manual trigger via `POST /api/admin/agents/trigger`
2. Loads all enabled data sources from the database
3. Collects signals via `RssFeedCollector` (CodeHollow.FeedReader) and `GitHubTopicsCollector` (GitHub Search API)
4. Deduplicates signals against existing entries and open proposals
5. Classifies each signal — either via Claude (if `ANTHROPIC_API_KEY` is set) or as-is
6. Caps results at `AGENTS_MAX_PROPOSALS_PER_RUN` and persists proposals
7. Writes a structured run log (visible in the Agents page)

**Trigger a scan manually:**

```bash
# Get a token
TOKEN=$(curl -s -X POST http://localhost/api/admin/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"<password>"}' | jq -r '.token')

# Trigger
curl -s -X POST http://localhost/api/admin/agents/trigger \
  -H "Authorization: Bearer $TOKEN"
```

---

## Seeded Data

On first startup the API runs EF migrations which seed:

**8 IoT technology entries** across all four quadrants:

| Technology | Quadrant | Ring |
|-----------|----------|------|
| MQTT | Connectivity Protocols | Adopt |
| LoRaWAN | Connectivity Protocols | Adopt |
| Matter | Connectivity Protocols | Trial |
| Thread | Connectivity Protocols | Trial |
| NB-IoT | Connectivity Protocols | Assess |
| AWS IoT Core | Edge Platforms | Adopt |
| Zephyr RTOS | Tools & Frameworks | Trial |
| DTDL | Standards & Techniques | Assess |

**7 RSS/GitHub data sources** pre-configured for IoT/connectivity content.
