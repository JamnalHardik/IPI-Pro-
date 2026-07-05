# Specimen Check-In — Full-Stack Technical Assignment

A vertical slice of IPI Pro's **Specimen Check-In** feature: a Vue.js 3 front-end connected to an ASP.NET Core 8 Web API, backed by PostgreSQL, fully containerised with Docker.

Lab technicians open manifests, mark specimens as received, flag missing or off-manifest bottles as discrepancies, and close manifests once reconciled — all with server-enforced multi-tenant data isolation.

---

## Quick Start

```bash
docker compose up
```

That's it. Three containers boot: PostgreSQL 16, the .NET 8 API (port 5000), and the Vue 3 dev server (port 5173).

Open **http://localhost:5173** in your browser.

The application is pre-seeded with two labs (Central Lab and North Lab), several manifests at different stages (InTransit, Open, Closed, ClosedWithDiscrepancy), and specimens in Pending, Received, Flagged, and Added states.

### In-App Tenant & User Switching

Click the **user avatar** (top right: `LT` / `T2`) to open a dropdown that lets you:

- **Switch tenant**: Toggle between Central Lab and North Lab — the manifest list refreshes immediately with only that tenant's data
- **Switch user**: Change between Lab Tech 1 (`tech1`) and Lab Tech 2 (`tech2`) — all subsequent actions are logged under the selected user

This demonstrates tenant isolation live: switch to North Lab and you'll see only `MF-2026-0043` and `MF-2026-0044`. Switch back to Central Lab and all your manifests return.

### Tab Navigation

Clicking the top navigation tabs (Check-In, Scan History, Manifests, Discrepancies) switches between views:
- **Check-In**: the full specimen check-in workflow
- **Other tabs**: placeholder views with a "Back to Check-In" link (only Check-In is in scope for this assignment)

---

## Tech Stack

| Layer | Choice | Why |
|---|---|---|
| **Backend** | ASP.NET Core 8 Web API | Required by the assignment; .NET 8 is the current LTS |
| **ORM** | Entity Framework Core 8 (Npgsql) | Code-first migrations; PostgreSQL is open-source and portable |
| **Database** | PostgreSQL 16 | Not in the brief but chosen over SQLite for real-world relevance; runs via Docker with no manual setup |
| **Frontend** | Vue 3 + Vite + TypeScript | Required; Composition API, SFCs, and TypeScript for type safety |
| **State** | Pinia | Lightweight, idiomatic Vue 3 store |
| **Styling** | Tailwind CSS | Utility-first, rapid development, consistent design language |
| **Testing (BE)** | xUnit + EF Core InMemory + MVC Test Host | Service-logic tests and full HTTP pipeline tests |
| **Testing (FE)** | Vitest + Vue Test Utils | Fast, Vite-native test runner for component tests |
| **Containerisation** | Docker + Compose | One-command startup; no local PostgreSQL or .NET SDK needed to run |

---

## Running Locally (Without Docker)

### Prerequisites
- .NET 8 SDK
- Node.js 20+
- PostgreSQL 16 (or change the connection string)

### Backend

```bash
cd backend/SpecimenCheckin/src/SpecimenCheckin.Api
dotnet ef database update    # apply migrations
dotnet run
# API listens on http://localhost:5000
```

### Frontend

```bash
cd frontend/specimen-check-in
npm install
npm run dev
# Dev server on http://localhost:5173, proxies /api → localhost:5000
```

---

## API Endpoints

All endpoints require `X-Tenant-Id` and `X-User-Id` headers.

| Method | Path | Description |
|---|---|---|
| `GET` | `/api/tenants` | List available labs (no tenant header required) |
| `GET` | `/api/manifests?status=Open` | List manifests for the current tenant |
| `GET` | `/api/manifests/{id}` | Manifest detail with specimens and discrepancies |
| `POST` | `/api/manifests/{id}/specimens/{sid}/receive` | Mark a specimen as received (idempotent) |
| `POST` | `/api/manifests/{id}/specimens/{sid}/flag` | Flag a specimen missing (creates Missing discrepancy) |
| `POST` | `/api/manifests/{id}/specimens` | Add an off-manifest specimen (creates OffManifest discrepancy) |
| `POST` | `/api/manifests/{id}/close` | Close manifest (422 if not fully reconciled) |

Errors return RFC 7807 `application/problem+json`.

---

## Running Tests

### Backend (14 tests)

```bash
cd backend/SpecimenCheckin
dotnet test
```

Covers: tenant isolation, cross-tenant 404s, idempotent receive, flag creates discrepancy, off-manifest creates discrepancy, close rejects unreconciled, close succeeds when reconciled, missing tenant header returns 400.

### Frontend (8 tests)

```bash
cd frontend/specimen-check-in
npm run test
```

Covers: StatusPill renders correct colour/text + checkmark icon for all 6 specimen/manifest statuses, KpiCards displays correct counts.

---

## Project Structure

```
.
├── docker-compose.yml          # PostgreSQL + backend + frontend
├── .gitignore
├── README.md
├── backend/
│   └── SpecimenCheckin/
│       ├── Dockerfile
│       ├── SpecimenCheckin.sln
│       ├── src/SpecimenCheckin.Api/
│       │   ├── Controllers/    # ManifestsController.cs, TenantsController.cs
│       │   ├── Data/           # AppDbContext.cs, SeedData.cs, Migrations/
│       │   ├── DTOs/           # Request & response types
│       │   ├── Middleware/      # TenantMiddleware.cs
│       │   ├── Models/         # Lab, Clinic, Manifest, Specimen, Discrepancy, CheckInEvent
│       │   ├── Services/       # ManifestService.cs (business logic)
│       │   └── Program.cs      # Startup, DI, CORS, middleware pipeline
│       └── tests/SpecimenCheckin.Api.Tests/
│           ├── ManifestServiceTests.cs
│           └── TenantIsolationTests.cs
└── frontend/
    └── specimen-check-in/
        ├── Dockerfile
        ├── src/
        │   ├── api/            # manifestApi.ts (Axios client)
        │   ├── assets/         # main.css (Tailwind directives)
        │   ├── components/     # 6 Vue SFCs
        │   ├── stores/         # useCheckInStore.ts (Pinia)
        │   └── types/          # TypeScript interfaces
        ├── __tests__/          # Vitest component tests
        └── vite.config.ts
```

---

## Architecture Write-Up

### Azure Topology

The application would be deployed as follows:

```
Azure Front Door / CDN
    │
    ▼
Azure App Service (Linux)  ────  Azure SQL (private endpoint)
    │
    ▼ (on manifest close)
Azure Storage Queue  ────▶  Azure Functions (Consumption)
    │                              │
    │                              ▼
    │                      Azure Blob Storage
    │                      (reconciliation summary)
    │
    ▼
Azure Cosmos DB  (immutable audit log)
```

- **App Service** hosts the ASP.NET Core API. Auto-scale rules based on HTTP queue depth.
- **Azure SQL** with VNet integration and private endpoints — no public exposure.
- **Storage Queue + Functions** for async reconciliation summaries on manifest close. The API drops a message; a function picks it up, generates a PDF/JSON summary, and writes it to Blob.
- **Cosmos DB** for the immutable event log (every CheckInEvent is appended, never mutated, partitioned by tenant).
- **Front Door/CDN** in front of the static front-end (served from Blob Storage or a second App Service).

### Session & State

User sessions would be managed via **JWT tokens issued by Azure AD B2C** (or another OIDC provider). The token carries the tenant claim (`tid`) and user identity (`sub`). On every request, `TenantMiddleware` validates the token and extracts the tenant ID — no server-side session state, making the API stateless and trivially scalable across App Service instances behind a load balancer. If sticky sessions are ever needed, Application Request Routing cookies on App Service can provide affinity without code changes.

### HIPAA-Aware Data Handling

1. **Encryption in transit**: TLS 1.2+ enforced at Azure Front Door, App Service, and between App Service ↔ Azure SQL via private VNet.
2. **Encryption at rest**: Azure SQL TDE (Transparent Data Encryption) enabled by default. Blob Storage encrypted at rest. Cosmos DB encrypted at rest.
3. **Patient data in logs**: No patient names, specimen codes, or PHI are ever logged at `Information` level. Structured logging uses message templates that reference ids, not values. Log sinks (Application Insights) are configured to scrub or mask sensitive fields.
4. **Minimal data access**: API endpoints return only what the UI needs. No full entity graphs are serialised — DTOs shape the response.
5. **Auditability**: Every action writes to `CheckInEvents` (and, in production, Cosmos DB) with the acting user ID and timestamp, providing a tamper-evident chain.

### Tenant Isolation

Isolation is enforced at the **service layer**, not just the database:

1. Every request must carry `X-Tenant-Id`. The `TenantMiddleware` rejects requests without it (HTTP 400).
2. The controller extracts `tenantId` from `HttpContext.Items` and passes it to every service method.
3. Every EF Core query in `ManifestService` filters by `LabId` — manifests, specimens, and discrepancies are all scoped to the requesting tenant via `WHERE LabId = @tenantId` (reached through `Manifest.LabId` for specimens and discrepancies).
4. On writes (receive, flag, close), the service first loads the manifest and checks `manifest.LabId == tenantId` — returning `null` (→ HTTP 404) on mismatch.

**Testing tenant isolation as the codebase grows**: Every new query method in the service must include a corresponding cross-tenant test that asserts the method returns null/empty for a different tenant. A shared test base class seeded with two labs makes this a one-line addition per endpoint.

### If I Had Two More Days

1. **Optimistic concurrency**: Add a `RowVersion` column to `Manifest` and use it as an ETag in API responses. On `POST .../receive` and `POST .../close`, include `If-Match` header validation to prevent two technicians from overwriting each other's work.
2. **End-to-end test**: A Cypress/Playwright script covering the happy path: select manifest → receive specimen → flag specimen → add off-manifest → verify counts → close manifest.
3. **Background reconciliation summary**: Wire up an Azure Functions worker (or a local `IHostedService` stand-in) that picks up close events from a queue/channel and generates a summary artifact.

---

## Design Decisions & Trade-Offs

- **PostgreSQL over SQLite**: Chosen to mirror a real production database with proper concurrency, JSON support, and real-world migration behaviour. Docker makes it zero-fuss for the reviewer.
- **No repository pattern**: The service directly uses `AppDbContext`. EF Core's `DbSet<T>` already implements the Repository + Unit of Work patterns. Extra abstraction here adds indirection without value for this slice.
- **Single-page layout**: No Vue Router. The Check-In screen shows the manifest worklist and detail panel together — routing adds complexity without a user-facing benefit for this vertical slice.
- **DTOs vs exposing entities**: DTOs decouple the API contract from the database schema and prevent over-posting and accidental serialisation of navigation properties.
- **Idempotent receive**: If a specimen is already `Received` or `Added`, calling receive again returns the current state without creating duplicate `CheckInEvent` records — safe for barcode re-scans.

---

## Seed Data Map

Tenant IDs are **fixed** so the UI tenant switcher references them predictably:

| Lab | Tenant ID | Manifests |
|---|---|---|
| Central Lab | `00000000-0000-0000-0000-000000000001` | 5 manifests across all statuses |
| North Lab | `00000000-0000-0000-0000-000000000002` | 2 manifests (Open + InTransit) |

### Manifest Details

| Lab | Manifest | Status | Specimens |
|---|---|---|---|
| Central Lab | MF-2026-0042 | Open | 5 Received, 1 Pending, 1 Flagged, 1 Added — Missing + OffManifest discrepancies |
| Central Lab | MF-2026-0045 | Open | 3 Received — ready to close |
| Central Lab | MF-2026-0041 | InTransit | 5 Pending |
| Central Lab | MF-2026-0040 | Closed | 6 Received |
| Central Lab | MF-2026-0039 | ClosedWithDiscrepancy | 3 Received, 1 Flagged |
| North Lab | MF-2026-0043 | Open | 1 Received, 2 Pending |
| North Lab | MF-2026-0044 | InTransit | 4 Pending |
