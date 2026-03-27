# GymBackend

A REST API for tracking gym workouts. Users define reusable exercises, build named workout templates, log sessions with per-set weight and rep data, and track personal bests over time.

## Features

- **JWT authentication** — register, login, bearer token on all protected routes
- **Exercises** — create and manage a personal exercise library with muscle group tagging
- **Workout templates** — build reusable blueprints with default sets/reps and day-of-week scheduling
- **Sessions** — start sessions from a template or ad-hoc; mark complete when done
- **Set logging** — log weight and reps per set with automatic personal best detection
- **Personal bests** — tracked per exercise; updated instantly when a new weight record is set
- **Input validation** — all request DTOs validated with data annotations
- **Rate limiting** — auth endpoints capped at 10 requests/minute
- **Global error handling** — consistent JSON error responses across all endpoints

## Tech Stack

| | |
|---|---|
| Runtime | .NET 9 / ASP.NET Core |
| ORM | Entity Framework Core 9 |
| Database | SQLite |
| Auth | JWT bearer tokens |
| Password hashing | BCrypt.Net-Next |
| API docs | OpenAPI (available at `/openapi/v1.json` in Development) |

## Architecture

```
Controllers  →  Services  →  Repositories  →  EF Core  →  SQLite
     ↑               ↑
   DTOs          Exceptions
```

| Layer | Folder | Responsibility |
|---|---|---|
| HTTP | `Controllers/` | Route handling, auth, delegates to services |
| Business logic | `Services/` | Validation, personal best calculation, DTO mapping |
| Data access | `Repositories/` | EF Core queries, always user-scoped |
| Domain | `Models/` | EF-mapped entities |
| Contracts | `DTOs/` | Request/response shapes |

### Domain model

```
User ──< Exercise
User ──< WorkoutTemplate ──< TemplateExercise >── Exercise
User ──< WorkoutSession  ──< SessionExercise  >── Exercise
                                SessionExercise ──< Set
WorkoutTemplate ──< WorkoutSession   (nullable FK — supports ad-hoc sessions)
Exercise ──── PersonalBestSet        (nullable FK → Set)
```

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [EF Core CLI tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet): `dotnet tool install --global dotnet-ef`

### 1. Clone and restore

```bash
git clone <repo-url>
cd GymBackend
dotnet restore
```

### 2. Configure JWT secrets

The JWT key is stored in .NET user secrets (not committed to source control). Run these once:

```bash
dotnet user-secrets set "Jwt:Key" "your-secret-key-at-least-32-characters-long"
dotnet user-secrets set "Jwt:Issuer" "GymBackend"
dotnet user-secrets set "Jwt:Audience" "GymBackendUsers"
```

### 3. Apply migrations

```bash
dotnet ef database update
```

This creates `gymtracker.db` in the project folder.

### 4. Run

```bash
dotnet run
# API available at http://localhost:5195
# OpenAPI schema at http://localhost:5195/openapi/v1.json
```

## API Reference

All endpoints except auth require a `Authorization: Bearer <token>` header.

### Auth

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | No | Create account, returns JWT |
| `POST` | `/api/auth/login` | No | Login, returns JWT |

**Register / Login body:**
```json
{ "email": "user@example.com", "password": "secret123", "username": "luke" }
```

---

### Exercises

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/exercises` | List all exercises for the current user |
| `GET` | `/api/exercises/{id}` | Get one exercise (includes personal best weight) |
| `POST` | `/api/exercises` | Create an exercise |
| `PUT` | `/api/exercises/{id}` | Update name, muscle group, or notes |
| `DELETE` | `/api/exercises/{id}` | Delete (blocked if used in any session) |

---

### Templates

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/templates` | List all templates |
| `GET` | `/api/templates/{id}` | Get template with exercises |
| `POST` | `/api/templates` | Create a template |
| `PUT` | `/api/templates/{id}` | Update name, description, day of week |
| `DELETE` | `/api/templates/{id}` | Delete template |
| `POST` | `/api/templates/{id}/exercises` | Add an exercise to a template |
| `DELETE` | `/api/templates/{id}/exercises/{exerciseId}` | Remove exercise from template |
| `PUT` | `/api/templates/{id}/exercises/reorder` | Update exercise order |

---

### Sessions

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/sessions` | List sessions (supports `?from=&to=` date filters) |
| `GET` | `/api/sessions/{id}` | Get full session with exercises and logged sets |
| `POST` | `/api/sessions` | Create session (from template or blank) |
| `PATCH` | `/api/sessions/{id}/complete` | Mark session as completed |
| `DELETE` | `/api/sessions/{id}` | Delete session |
| `POST` | `/api/sessions/{id}/exercises` | Add an exercise to a live session |
| `DELETE` | `/api/sessions/{id}/exercises/{sessionExerciseId}` | Remove exercise from session |

---

### Sets

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/sessions/{id}/exercises/{seId}/sets` | Log a set (auto-detects personal best) |
| `PUT` | `/api/sessions/{id}/exercises/{seId}/sets/{setId}` | Edit a set (recalculates personal best) |
| `DELETE` | `/api/sessions/{id}/exercises/{seId}/sets/{setId}` | Delete a set (recalculates personal best) |

**Log set body:**
```json
{ "setNumber": 1, "weightKg": 100.0, "reps": 5 }
```

---

