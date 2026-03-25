# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with this repository.

## Project overview

Gym workout tracking REST API. Users can define reusable exercises, build named workout
templates (e.g. "Chest Day"), schedule those templates across a week, log actual workout
sessions with per-set weight and rep data, and track progress over time including personal
bests and historical graphs.

Target users: personal use + small group (friends/family). Not built for scale.

## Stack

- **Runtime:** .NET 9 / ASP.NET Core
- **ORM:** Entity Framework Core 9 + SQLite
- **Auth:** JWT bearer tokens + BCrypt.Net-Next password hashing
- **Docs:** OpenAPI (served at `/openapi/v1.json` in Development only)

## Commands
```bash
# Build
dotnet build

# Run (HTTP on localhost:5195)
dotnet run

# Run with HTTPS
dotnet run --launch-profile https

# Add a migration
dotnet ef migrations add <MigrationName>

# Apply migrations / create DB
dotnet ef database update
```

## Architecture

### Layer structure

| Folder | Purpose |
|---|---|
| `Models/` | Domain entities (EF Core mapped classes) |
| `Data/AppDbContext.cs` | EF Core context — all DbSets, relationship config, unique indexes |
| `DTOs/` | Request and response shapes — never expose raw models to clients |
| `Interfaces/` | Repository and service contracts |
| `Repositories/` | EF Core data access implementations |
| `Services/` | Business logic (personal best detection, progress calculation, etc.) |
| `Controllers/` | Thin HTTP layer — delegates everything to services |

### Domain model
```
User ──< Exercise
User ──< WorkoutTemplate ──< TemplateExercise >── Exercise
User ──< WorkoutSession  ──< SessionExercise  >── Exercise
                                SessionExercise ──< Set
WorkoutTemplate ──< WorkoutSession (nullable FK — session can be unplanned)
```

### Key design decisions

- Exercises are **per-user**, not global. A user creates an exercise once and reuses it
  across any number of templates and sessions via join tables.
- `WorkoutTemplate` is a **blueprint** — it holds default sets/reps but no actual weights.
  Weights only live on `Set`.
- `WorkoutSession.TemplateId` is **nullable** — supports ad-hoc sessions not based on
  any template. On template delete, FK is set to null (not cascade delete).
- `Set.IsPersonalBest` is **denormalized** — computed at log time by comparing against
  all prior sets for that exercise for that user. Stored so dashboard queries stay fast.
- All data is **user-scoped** — every query must filter by the UserId claim from the JWT.
  No user should ever see another user's data.

### Known typos to fix before first migration is used in production

- `TemplateExercise.DeffultSets` / `DeffultReps` → `DefaultSets` / `DefaultReps`
- `WorkoutSession.ScheduelDate` → `ScheduledDate`

## Build plan

Work through these phases in order. Do not skip ahead — each phase depends on the last.

### Phase 1 — Foundation (done)
- [x] Domain models
- [x] AppDbContext with relationships and constraints
- [x] SQLite connection string in appsettings.json
- [x] InitialCreate migration
- [ ] Fix known model typos and re-migrate

### Phase 2 — Auth
- [ ] `POST /api/auth/register` — hash password with BCrypt, return JWT
- [ ] `POST /api/auth/login` — verify hash, return JWT
- [ ] JWT middleware wired up in Program.cs
- [ ] Helper to extract UserId from token claims (used in every controller)

### Phase 3 — Exercises
Simplest entity, no complex relationships. Good first controller to build.
- [ ] `GET    /api/exercises` — list all exercises for the authenticated user
- [ ] `POST   /api/exercises` — create a new exercise
- [ ] `GET    /api/exercises/{id}` — get one (must belong to user)
- [ ] `PUT    /api/exercises/{id}` — edit name, muscle group, notes
- [ ] `DELETE /api/exercises/{id}` — delete (guard: check it's not used in any session)

### Phase 4 — Workout templates
- [ ] `GET    /api/templates` — list all templates for user
- [ ] `POST   /api/templates` — create template (name, optional day of week)
- [ ] `GET    /api/templates/{id}` — get template with its exercises
- [ ] `PUT    /api/templates/{id}` — edit template metadata
- [ ] `DELETE /api/templates/{id}`
- [ ] `POST   /api/templates/{id}/exercises` — add an exercise to a template
- [ ] `DELETE /api/templates/{id}/exercises/{exerciseId}` — remove exercise from template
- [ ] `PUT    /api/templates/{id}/exercises/reorder` — update OrderIndex values

### Phase 5 — Workout sessions
- [ ] `GET    /api/sessions` — list sessions (support ?from=&to= date filters)
- [ ] `POST   /api/sessions` — create session (from template or blank)
- [ ] `GET    /api/sessions/{id}` — get full session with exercises and sets
- [ ] `PATCH  /api/sessions/{id}/complete` — mark session as completed
- [ ] `DELETE /api/sessions/{id}`
- [ ] `POST   /api/sessions/{id}/exercises` — add exercise to a live session
- [ ] `DELETE /api/sessions/{id}/exercises/{sessionExerciseId}`

### Phase 6 — Sets (core logging loop)
- [ ] `POST   /api/sessions/{id}/exercises/{sessionExerciseId}/sets` — log a set
        → on creation: check if WeightKg is a new personal best for this exercise,
          set IsPersonalBest = true and clear flag on any previous best if so
- [ ] `PUT    /api/sessions/{id}/exercises/{sessionExerciseId}/sets/{setId}` — edit a set
- [ ] `DELETE /api/sessions/{id}/exercises/{sessionExerciseId}/sets/{setId}`

### Phase 7 — Progress and dashboard
- [ ] `GET /api/exercises/{id}/history` — all sets for an exercise over time (for graphs)
- [ ] `GET /api/exercises/{id}/personal-best` — heaviest set ever logged
- [ ] `GET /api/dashboard/week` — this week's scheduled sessions and completion status
- [ ] `GET /api/dashboard/recent` — last N completed sessions

### Phase 8 — Polish
- [ ] Rate limiting (same middleware pattern as todo app)
- [ ] Global error handling middleware
- [ ] Input validation on all DTOs (data annotations or FluentValidation)
- [ ] Review all delete behaviours — decide cascade vs restrict vs set-null per relationship

## Patterns to follow

These match the conventions established in the todo API this project is based on.

- **Repository pattern** — all EF queries go through a repository, never directly in controllers
- **Services/interfaces** — business logic lives in services, controllers only call services
- **DTOs** — separate request and response DTOs per entity, never return raw EF models
- **JWT user scoping** — extract `UserId` from token in every controller action, pass it
  down to the service/repository so the query is always filtered to the current user
- **Async throughout** — all repository and service methods are async/await