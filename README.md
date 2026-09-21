# Threaded Comments

A single-page application for leaving comments with replies (cascading
threads), CAPTCHA protection and file attachments. Built as a test assignment:
**.NET Web API + EF Core + MS SQL** on the backend and **Vue 3** on the frontend,
packaged with Docker Compose.

---

## Features

### Comment form
- **User Name** — required, latin letters and digits only.
- **E-mail** — required, validated e-mail format.
- **Home page** — optional, validated URL.
- **CAPTCHA** — required, image-based challenge (SkiaSharp-generated,
  stored in Redis with TTL, single-use, answer compared via HMAC hash).
- **Text** — required, supports a restricted allow-list of HTML tags:
  `<a href="" title="">`, `<code>`, `<i>`, `<strong>`.
- **Attachment** (optional):
  - Image (JPG / GIF / PNG), auto-resized to max **320x240** (client canvas
    + server SkiaSharp re-processing);
  - Text file (TXT), max **100 KB**.
- **Preview** without page reload.
- Toolbar buttons to insert `[i]`, `[strong]`, `[code]`, `[a]` tags.

### Comments page
- Cascading (recursive) replies.
- Sorting on top-level comments by **User Name**, **E-mail** and **Date**
  (ASC / DESC). Default sort is **LIFO** (newest first).
- Pagination — **25 comments** per page.
- Attachments preview with visual effects (image lightbox, text modal).

### Security
- **XSS** — server-side HTML sanitization (allow-list of tags/attributes and
  URL schemes) plus client-side validation/preview.
- **SQL injection** — EF Core parameterized queries only, no raw SQL string
  concatenation.
- **CAPTCHA** — required on comment creation, single-use tokens.
- **File storage** — path-traversal guard, extension allow-list, size limits.

---

## Tech stack

| Layer     | Technology |
|-----------|------------|
| Backend   | .NET 10, ASP.NET Core Minimal API, EF Core 10 |
| Database  | MS SQL Server |
| Cache     | Redis (CAPTCHA storage) |
| Frontend  | Vue 3, Vite, Bulma (CSS) |
| Container | Docker, Docker Compose |
| Image lib | SkiaSharp (CAPTCHA + image resize) |
| Validation| FluentValidation |

**Architecture:** Clean Architecture (`Core` → `Application` → `Persistence` →
`API`), CQRS-style dispatcher with pipeline validation behavior.

---

## Project structure

```
.
├── backend/
│   ├── dZENcode.Core/          # domain entities
│   ├── dZENcode.Application/   # use cases, validators, dispatcher
│   ├── dZENcode.Persistence/   # EF Core DbContext, configurations, migrations
│   └── dZENcode.API/           # minimal API endpoints, DI, exception handling
├── frontend/                   # Vue 3 SPA
├── db/                         # database schema files (see db/README.md)
├── docker-compose.yaml
└── .env.example                # environment template
```

---

## Getting started (Docker)

The whole stack (MS SQL, Redis, API, frontend) runs with a single command.

```bash
# 1. Copy the environment template and fill in the values
cp .env.example .env

# 2. Build and start everything
docker compose up --build
```

Once up:

- Frontend: <http://localhost:8080>
- API: <http://localhost:5046>

Database migrations are applied automatically on API startup.

### Environment variables

All secrets and connection settings are provided via environment variables
(see `.env.example`). Key groups:

- `ConnectionStrings__DzenConnectionString`, `ConnectionStrings__RedisConnectionString`
- `CaptchaOptions__SecretKey`, `CaptchaOptions__TTLMinutes`
- `StorageOptions__RootPath` (attachment storage path)
- `Cors__AllowedOrigins__*` (allowed frontend origins)
- `VITE_API_BASE_URL` (frontend → API base URL)

> **Note:** this repository contains no production hosting configuration.
> The stack is intended to be reproduced locally / on any VDS with Docker
> Compose using the command above.

---

## Running locally (without Docker)

### Backend
```bash
cd backend/dZENcode.API
dotnet restore
dotnet run
```
Requires a reachable MS SQL Server and Redis instance (see `.env.example`).

### Frontend
```bash
cd frontend
npm install
npm run dev
```

---

## API overview

| Method | Route                          | Description                              |
|--------|--------------------------------|------------------------------------------|
| GET    | `/comments`                    | Paged, sorted list of top-level comments |
| GET    | `/comments/{id}/replies`       | Replies for a comment                    |
| POST   | `/comments`                    | Create a comment (multipart/form-data)   |
| GET    | `/antiforgery/token`           | Generate an Antitiforgery Token          |
| GET    | `/captcha`                     | Generate a CAPTCHA challenge             |
| GET    | `/attachments/{**path}`        | Download / preview an attachment         |

### GET /comments

Returns a paged, sorted list of **top-level** comments.

Query parameter `Parameters` is a custom-bound tuple (a workaround for
minimal API model binding of complex types):

```
GET /comments?Parameters=(page,pageSize,sortBy,isDesc)
```

| Field      | Type                                                 | Notes |
|------------|------------------------------------------------------|-------|
| `page`     | int (1-based)                                        | page number |
| `pageSize` | int                                                  | items per page (UI uses `25`) |
| `sortBy`   | `CreatedAt` \| `Username` \| `Email`                 | enum |
| `isDesc`   | `true` \| `false`                                    | sort direction |

Example:

```
GET /comments?Parameters=(1,25,CreatedAt,true)
GET /comments?Parameters=(2,25,Username,false)
```

If the value cannot be parsed (or is omitted), the request **falls back to
defaults**: `page=1, pageSize=25, sortBy=CreatedAt, isDesc=true` (i.e. newest
first, LIFO). No error is returned in that case.

---

## Database schema

See [`db/README.md`](db/README.md):

- `db/schema-mssql.sql` — authoritative DDL (from EF Core migrations);
- `db/schema-mysql.sql` — MySQL-equivalent schema to open in MySQL Workbench.

---
