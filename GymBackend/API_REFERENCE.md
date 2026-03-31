# GymTracker API — Technical Reference

> **Audience:** Frontend developers. This document covers every endpoint, every DTO, auth, error shapes, and deployment configuration. It is intended to be a complete reference — no source code access required.

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Base URL & Headers](#2-base-url--headers)
3. [Authentication](#3-authentication)
4. [Error Responses](#4-error-responses)
5. [Endpoint Reference](#5-endpoint-reference)
   - [Auth](#51-auth)
   - [Exercises](#52-exercises)
   - [Templates](#53-templates)
   - [Sessions](#54-sessions)
   - [Sets](#55-sets)
   - [Dashboard](#56-dashboard)
6. [Data Model](#6-data-model)
7. [Architecture](#7-architecture)
8. [Personal Best System](#8-personal-best-system)
9. [Security & Rate Limiting](#9-security--rate-limiting)
10. [Deployment](#10-deployment)
11. [Full DTO Reference](#11-full-dto-reference)

---

## 1. Project Overview

GymTracker is a REST API for personal workout logging. It lets users:

- Define reusable **exercises** (e.g. "Bench Press", "Squat") that belong to them alone.
- Build named **workout templates** (e.g. "Chest Day") with a default exercise list and rep targets.
- Schedule and log **workout sessions** — either from a template or completely ad-hoc.
- Log individual **sets** with weight and reps, with automatic personal-best detection per exercise.
- Query **progress history** for any exercise over time, and view a **weekly dashboard**.

Target users: personal use and small groups (friends/family). Not designed for scale.

---

## 2. Base URL & Headers

| Environment | Base URL |
|---|---|
| Local dev | `http://localhost:5195` |
| Production (Render) | `https://<your-service>.onrender.com` |

All request and response bodies are `application/json`.

Every protected endpoint requires a bearer token:

```
Authorization: Bearer <jwt_token>
```

Dates use the format `"YYYY-MM-DD"` (JSON string, ISO 8601 date-only).
Datetimes use the format `"YYYY-MM-DDTHH:mm:ss.fffZ"` (UTC ISO 8601).

---

## 3. Authentication

### How it works

1. Register or log in → receive a JWT.
2. Include the JWT in every subsequent request as `Authorization: Bearer <token>`.
3. The token expires after **24 hours**. There is no refresh endpoint — the user must log in again.

### Token contents

The JWT payload contains:

| Claim | Description |
|---|---|
| `sub` | User's numeric ID (integer as string) |
| `unique_name` | Username |
| `email` | User email |
| `jti` | Unique token ID (GUID) |
| `exp` | Expiry (UTC Unix timestamp, 24h from issue) |

### Sending the token

```http
GET /api/exercises
Authorization: Bearer eyJhbGci...
```

### 401 responses

If the token is missing, invalid, or expired, the server returns `401 Unauthorized` with no body (this comes from the JWT middleware before the controller runs).

---

## 4. Error Responses

All error responses have the same shape:

```json
{ "message": "Human-readable description of the error." }
```

| HTTP Status | When it occurs |
|---|---|
| `400 Bad Request` | Validation failure on request body; duplicate email/username on register; exercise in use when deleting; exercise already in template |
| `401 Unauthorized` | Missing/invalid/expired JWT; wrong credentials on login |
| `404 Not Found` | Requested resource does not exist or belongs to another user |
| `422 Unprocessable Entity` | Model validation failed (data annotations) — returned automatically by ASP.NET Core with a validation error object, not the `{ message }` shape |
| `429 Too Many Requests` | Rate limit exceeded |
| `500 Internal Server Error` | Unhandled exception — always returns `{ "message": "An unexpected error occurred." }` |

> **Note on 422:** When data annotation validation fails (e.g. missing required field, out-of-range value), ASP.NET Core returns a `422` with the standard `ProblemDetails` shape:
> ```json
> {
>   "errors": { "fieldName": ["Error message."] },
>   "title": "One or more validation errors occurred.",
>   "status": 422
> }
> ```

---

## 5. Endpoint Reference

### 5.1 Auth

**Base route:** `/api/auth`
**Rate limit:** 5 requests per minute (per IP/host). Applies to both endpoints below.
**Auth required:** No.

---

#### `POST /api/auth/register`

Create a new account and receive a JWT.

**Request body:**
```json
{
  "username": "string",
  "email": "string",
  "password": "string"
}
```

| Field | Type | Rules |
|---|---|---|
| `username` | string | Required. 3–50 characters. Must be unique. |
| `email` | string | Required. Valid email format. Max 256 chars. Must be unique. |
| `password` | string | Required. 6–100 characters. |

**Response `200 OK`:**
```json
{
  "token": "eyJhbGci...",
  "username": "johndoe"
}
```

**Errors:**
- `400` — Email already registered / Username already taken.
- `422` — Validation failure (field missing, too short, invalid email format, etc.).

---

#### `POST /api/auth/login`

Authenticate and receive a JWT.

**Request body:**
```json
{
  "email": "string",
  "password": "string"
}
```

| Field | Type | Rules |
|---|---|---|
| `email` | string | Required. Valid email format. Max 256 chars. |
| `password` | string | Required. Max 256 chars. |

**Response `200 OK`:**
```json
{
  "token": "eyJhbGci...",
  "username": "johndoe"
}
```

**Errors:**
- `401` — Invalid email or password. (Intentionally vague — does not distinguish between wrong email vs. wrong password.)

---

### 5.2 Exercises

**Base route:** `/api/exercises`
**Auth required:** Yes (all endpoints).

Exercises are per-user. A user's exercises are the pool from which templates and sessions are built.

---

#### `GET /api/exercises`

List all exercises for the authenticated user, each with their current personal best weight.

**Response `200 OK`:** Array of `ExerciseDto`
```json
[
  {
    "id": 1,
    "name": "Bench Press",
    "muscleGroup": "Chest",
    "notes": "Keep elbows tucked",
    "personalBest": 102.5
  }
]
```

| Field | Type | Notes |
|---|---|---|
| `id` | int | |
| `name` | string | |
| `muscleGroup` | string | |
| `notes` | string | Empty string if not set |
| `personalBest` | float? | Heaviest weight ever logged for this exercise. `null` if no sets logged. |

---

#### `GET /api/exercises/{id}`

Get a single exercise by ID.

**Response `200 OK`:** `ExerciseDto` (same shape as list item above)

**Errors:** `404` — Exercise not found (or belongs to another user).

---

#### `POST /api/exercises`

Create a new exercise.

**Request body:**
```json
{
  "name": "Deadlift",
  "muscleGroup": "Back",
  "notes": "Optional notes"
}
```

| Field | Type | Rules |
|---|---|---|
| `name` | string | Required. Max 100 chars. |
| `muscleGroup` | string | Required. Max 100 chars. |
| `notes` | string | Optional. Max 500 chars. |

**Response `201 Created`:** `ExerciseDto`

Location header: `/api/exercises/{id}`

---

#### `PUT /api/exercises/{id}`

Replace exercise metadata. All fields are required.

**Request body:** Same shape as `POST /api/exercises`.

**Response `200 OK`:** Updated `ExerciseDto`

**Errors:** `404` — Exercise not found.

---

#### `DELETE /api/exercises/{id}`

Delete an exercise.

**Response `204 No Content`**

**Errors:**
- `404` — Exercise not found.
- `400` — Exercise is used in one or more workout sessions and cannot be deleted.

> **Business rule:** An exercise can be deleted even if it is in a template. It cannot be deleted if it has ever appeared in a logged session (i.e. `SessionExercise` rows exist for it). This guards against orphaning historical set data.

---

#### `GET /api/exercises/{id}/progress`

Get the full progress history for an exercise, grouped by date, for use in graphs.

**Response `200 OK`:** `ExerciseProgressDto`
```json
{
  "exerciseId": 1,
  "exerciseName": "Bench Press",
  "personalBest": 102.5,
  "history": [
    {
      "date": "2024-01-15",
      "bestWeight": 90.0,
      "totalReps": 24,
      "totalSets": 3
    },
    {
      "date": "2024-01-22",
      "bestWeight": 95.0,
      "totalReps": 21,
      "totalSets": 3
    }
  ]
}
```

| Field | Type | Notes |
|---|---|---|
| `exerciseId` | int | |
| `exerciseName` | string | |
| `personalBest` | float | `0` if no sets logged |
| `history` | array | One entry per session date, ordered ascending |
| `history[].date` | string (date) | The session's `scheduledDate`, or the set's `loggedAt` date if no scheduled date |
| `history[].bestWeight` | float | Heaviest set logged on that date |
| `history[].totalReps` | int | Sum of all reps logged on that date |
| `history[].totalSets` | int | Count of all sets logged on that date |

**Errors:** `404` — Exercise not found.

---

### 5.3 Templates

**Base route:** `/api/templates`
**Auth required:** Yes (all endpoints).

Templates are workout blueprints. They define which exercises to do and how many default sets/reps, but hold no weight data. When a session is created from a template, the exercises are copied in.

---

#### `GET /api/templates`

List all templates for the user (summary, no exercise list).

**Response `200 OK`:** Array of `TemplateDto`
```json
[
  {
    "id": 1,
    "name": "Chest Day",
    "description": "Heavy push session",
    "dayOfWeek": "Monday"
  }
]
```

| Field | Type | Notes |
|---|---|---|
| `id` | int | |
| `name` | string | |
| `description` | string | Empty string if not set |
| `dayOfWeek` | string | e.g. `"Monday"`. Empty string if not set. Free text — not validated as a day name. |

---

#### `GET /api/templates/{id}`

Get a single template with its full exercise list.

**Response `200 OK`:** `TemplateDetailDto`
```json
{
  "id": 1,
  "name": "Chest Day",
  "description": "Heavy push session",
  "dayOfWeek": "Monday",
  "exercises": [
    {
      "templateExerciseId": 3,
      "exerciseId": 1,
      "exerciseName": "Bench Press",
      "muscleGroup": "Chest",
      "orderIndex": 0,
      "defaultSets": 4,
      "defaultReps": 8
    }
  ]
}
```

Exercises are returned ordered by `orderIndex` ascending.

**Errors:** `404` — Template not found.

---

#### `POST /api/templates`

Create a new template (no exercises yet).

**Request body:**
```json
{
  "name": "Chest Day",
  "description": "Heavy push session",
  "dayOfWeek": "Monday"
}
```

| Field | Type | Rules |
|---|---|---|
| `name` | string | Required. Max 100 chars. |
| `description` | string | Optional. Max 500 chars. |
| `dayOfWeek` | string | Optional. Max 20 chars. Free text. |

**Response `201 Created`:** `TemplateDto`

---

#### `PUT /api/templates/{id}`

Update template metadata.

**Request body:** Same shape as `POST /api/templates`.

**Response `200 OK`:** `TemplateDto`

**Errors:** `404` — Template not found.

---

#### `DELETE /api/templates/{id}`

Delete a template. Any sessions previously created from this template are NOT deleted — their `templateId` is set to `null` instead (set-null on delete).

**Response `204 No Content`**

**Errors:** `404` — Template not found.

---

#### `POST /api/templates/{id}/exercises`

Add an exercise to a template.

**Request body:**
```json
{
  "exerciseId": 1,
  "defaultSets": 4,
  "defaultReps": 8,
  "orderIndex": 0
}
```

| Field | Type | Rules |
|---|---|---|
| `exerciseId` | int | Required. Must be an exercise owned by the user. |
| `defaultSets` | int | 1–100. Defaults to `3` if omitted. |
| `defaultReps` | int | 1–100. Defaults to `10` if omitted. |
| `orderIndex` | int | 0–1000. Used for display ordering. Defaults to `0`. |

**Response `200 OK`:** `TemplateExerciseDto`
```json
{
  "templateExerciseId": 3,
  "exerciseId": 1,
  "exerciseName": "Bench Press",
  "muscleGroup": "Chest",
  "orderIndex": 0,
  "defaultSets": 4,
  "defaultReps": 8
}
```

**Errors:**
- `404` — Template not found / Exercise not found.
- `400` — Exercise is already in this template.

---

#### `DELETE /api/templates/{id}/exercises/{exerciseId}`

Remove an exercise from a template.

> **Note:** The `{exerciseId}` here is the **exercise's own ID** (from `GET /api/exercises`), not the `templateExerciseId`.

**Response `204 No Content`**

**Errors:** `404` — Exercise not found in template.

---

#### `PUT /api/templates/{id}/exercises/reorder`

Batch-update the `orderIndex` of exercises within a template.

**Request body:**
```json
{
  "items": [
    { "templateExerciseId": 3, "orderIndex": 0 },
    { "templateExerciseId": 7, "orderIndex": 1 },
    { "templateExerciseId": 12, "orderIndex": 2 }
  ]
}
```

| Field | Type | Rules |
|---|---|---|
| `items` | array | Required. |
| `items[].templateExerciseId` | int | Required. The `templateExerciseId` from `GET /api/templates/{id}`. |
| `items[].orderIndex` | int | 0–1000. |

Items not included in the array are unaffected. Items with IDs that don't belong to this template are silently ignored.

**Response `204 No Content`**

**Errors:** `404` — Template not found.

---

### 5.4 Sessions

**Base route:** `/api/sessions`
**Auth required:** Yes (all endpoints).

A session represents one actual workout. It can be based on a template (exercises pre-populated) or completely ad-hoc.

---

#### `GET /api/sessions`

List sessions, optionally filtered by date range.

**Query parameters:**

| Parameter | Type | Description |
|---|---|---|
| `from` | date string | Optional. `YYYY-MM-DD`. Include only sessions with `scheduledDate >= from`. |
| `to` | date string | Optional. `YYYY-MM-DD`. Include only sessions with `scheduledDate <= to`. |

Filters apply to `scheduledDate`. Sessions with a `null` scheduled date are only included if no date filter is applied.

**Response `200 OK`:** Array of `SessionDto`
```json
[
  {
    "id": 5,
    "scheduledDate": "2024-01-22",
    "isCompleted": true,
    "completedAt": "2024-01-22T09:45:00.000Z",
    "notes": "Felt strong today",
    "templateId": 1,
    "templateName": "Chest Day"
  }
]
```

| Field | Type | Notes |
|---|---|---|
| `id` | int | |
| `scheduledDate` | string (date)? | `null` for ad-hoc sessions |
| `isCompleted` | bool | |
| `completedAt` | string (datetime)? | UTC. `null` until session is completed. |
| `notes` | string | Empty string if not set |
| `templateId` | int? | `null` if not created from a template, or if original template was deleted |
| `templateName` | string? | Current name of the template. `null` if no template. |

---

#### `GET /api/sessions/{id}`

Get full session details including all exercises and their logged sets.

**Response `200 OK`:** `SessionDetailDto` (extends `SessionDto`)
```json
{
  "id": 5,
  "scheduledDate": "2024-01-22",
  "isCompleted": false,
  "completedAt": null,
  "notes": "",
  "templateId": 1,
  "templateName": "Chest Day",
  "exercises": [
    {
      "sessionExerciseId": 11,
      "exerciseId": 1,
      "exerciseName": "Bench Press",
      "muscleGroup": "Chest",
      "orderIndex": 0,
      "sets": [
        {
          "id": 20,
          "setNumber": 1,
          "weightKg": 80.0,
          "reps": 10,
          "isPersonalBest": false,
          "loggedAt": "2024-01-22T09:15:00.000Z"
        }
      ]
    }
  ]
}
```

Exercises are ordered by `orderIndex` ascending. Sets within each exercise are ordered by `setNumber` ascending.

**Errors:** `404` — Session not found.

---

#### `POST /api/sessions`

Create a new session, optionally from a template.

**Request body:**
```json
{
  "templateId": 1,
  "scheduledDate": "2024-01-22",
  "notes": "Optional notes"
}
```

| Field | Type | Rules |
|---|---|---|
| `templateId` | int? | Optional. If provided, exercises are cloned from the template. |
| `scheduledDate` | string (date)? | Optional. `null` for ad-hoc sessions. |
| `notes` | string | Optional. Max 1000 chars. |

**Behaviour when `templateId` is provided:**
- The template must belong to the authenticated user.
- All exercises from the template are added to the session automatically, preserving `orderIndex`.
- Sets are NOT pre-populated — the session starts empty, waiting for actual logged sets.

**Response `201 Created`:** `SessionDetailDto`

**Errors:** `404` — Template not found.

---

#### `PATCH /api/sessions/{id}/complete`

Mark a session as completed. Sets `isCompleted = true` and `completedAt = now (UTC)`.

Calling this on an already-completed session is allowed and idempotent (overwrites `completedAt`).

**Response `200 OK`:** `SessionDto`

**Errors:** `404` — Session not found.

---

#### `DELETE /api/sessions/{id}`

Delete a session and all its exercises and sets (cascade delete).

**Response `204 No Content`**

**Errors:** `404` — Session not found.

---

#### `POST /api/sessions/{id}/exercises`

Add an exercise to a live session.

**Request body:**
```json
{
  "exerciseId": 1,
  "orderIndex": 2
}
```

| Field | Type | Rules |
|---|---|---|
| `exerciseId` | int | Required. Must be an exercise owned by the user. |
| `orderIndex` | int | 0–1000. Defaults to `0`. |

**Response `200 OK`:** `SessionExerciseDto`
```json
{
  "sessionExerciseId": 11,
  "exerciseId": 1,
  "exerciseName": "Bench Press",
  "muscleGroup": "Chest",
  "orderIndex": 2,
  "sets": []
}
```

**Errors:** `404` — Session not found / Exercise not found.

---

#### `DELETE /api/sessions/{id}/exercises/{sessionExerciseId}`

Remove an exercise from a session. Also deletes all sets logged for that exercise in this session.

> **Note:** `{sessionExerciseId}` is the `sessionExerciseId` field from `SessionExerciseDto`, not the `exerciseId`.

**Response `204 No Content`**

**Errors:** `404` — Exercise not found in session.

---

### 5.5 Sets

Sets live under the sessions route. They represent a single logged set (e.g. "Set 2: 90kg × 8 reps").

---

#### `POST /api/sessions/{sessionId}/exercises/{sessionExerciseId}/sets`

Log a new set.

**Request body:**
```json
{
  "setNumber": 1,
  "weightKg": 90.0,
  "reps": 8
}
```

| Field | Type | Rules |
|---|---|---|
| `setNumber` | int | Required. 1–100. Caller-defined ordering (e.g. "Set 1", "Set 2"). |
| `weightKg` | float | 0.5–1000. Weight in kilograms. |
| `reps` | int | 1–200. |

**Business logic — personal best detection:**
- On every log, the new weight is compared against all previously logged weights for this exercise across all sessions.
- If `weightKg` exceeds the current best, `isPersonalBest` is set to `true` on the new set, and the previous best set's flag is cleared.
- If `weightKg` does not exceed the current best, `isPersonalBest` is `false`.

**Response `200 OK`:** `SetDto`
```json
{
  "id": 20,
  "setNumber": 1,
  "weightKg": 90.0,
  "reps": 8,
  "isPersonalBest": true,
  "loggedAt": "2024-01-22T09:15:00.000Z"
}
```

**Errors:** `404` — Session exercise not found.

---

#### `PUT /api/sessions/{sessionId}/exercises/{sessionExerciseId}/sets/{setId}`

Edit a logged set. Triggers a full personal-best recalculation across all sets for this exercise.

**Request body:** Same shape as `POST` (all fields required).

**Response `200 OK`:** `SetDto`

**Errors:** `404` — Session exercise not found / Set not found.

---

#### `DELETE /api/sessions/{sessionId}/exercises/{sessionExerciseId}/sets/{setId}`

Delete a logged set. If the deleted set was the personal best, the next-heaviest set is promoted to personal best.

**Response `204 No Content`**

**Errors:** `404` — Session exercise not found / Set not found.

---

### 5.6 Dashboard

**Base route:** `/api/dashboard`
**Auth required:** Yes (all endpoints).

---

#### `GET /api/dashboard/week`

Get this week's sessions (Monday–Sunday, UTC).

**Response `200 OK`:** `WeeklyDashboardDto`
```json
{
  "weekStart": "2024-01-22",
  "weekEnd": "2024-01-28",
  "totalScheduled": 3,
  "totalCompleted": 1,
  "sessions": [
    {
      "id": 5,
      "scheduledDate": "2024-01-22",
      "isCompleted": true,
      "completedAt": "2024-01-22T09:45:00.000Z",
      "notes": "",
      "templateId": 1,
      "templateName": "Chest Day"
    }
  ]
}
```

| Field | Type | Notes |
|---|---|---|
| `weekStart` | string (date) | Monday of the current UTC week |
| `weekEnd` | string (date) | Sunday of the current UTC week |
| `totalScheduled` | int | Count of sessions in this date range |
| `totalCompleted` | int | Count of completed sessions in this date range |
| `sessions` | array | `SessionDto` array — sessions whose `scheduledDate` falls in the week |

---

#### `GET /api/dashboard/recent?count=5`

Get the most recently completed sessions.

**Query parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `count` | int | `5` | Number of sessions to return |

Returns sessions ordered by `completedAt` descending (most recent first).

**Response `200 OK`:** Array of `SessionDto`

---

## 6. Data Model

### Entity overview

```
User
 ├── Exercise (many, per-user)
 ├── WorkoutTemplate (many, per-user)
 │    └── TemplateExercise (join: Template × Exercise)
 └── WorkoutSession (many, per-user)
      ├── WorkoutTemplate? (nullable FK — template this session was created from)
      └── SessionExercise (join: Session × Exercise)
           └── Set (the actual logged weights/reps)
```

### User

| Field | Type | Notes |
|---|---|---|
| `Id` | int | PK |
| `Username` | string | Unique |
| `Email` | string | Unique |
| `PasswordHash` | string | BCrypt hash — never returned to client |
| `CreatedAt` | DateTime (UTC) | Set at creation |

### Exercise

| Field | Type | Notes |
|---|---|---|
| `Id` | int | PK |
| `Name` | string | |
| `MuscleGroup` | string | |
| `Notes` | string | |
| `UserId` | int | FK → User |
| `PersonalBestSetId` | int? | FK → Set (nullable, denormalized pointer to the current PB set) |

### WorkoutTemplate

| Field | Type | Notes |
|---|---|---|
| `Id` | int | PK |
| `Name` | string | |
| `Description` | string | |
| `DayOfWeek` | string | Free text |
| `CreatedAt` | DateTime (UTC) | |
| `UserId` | int | FK → User |

### TemplateExercise (join table)

| Field | Type | Notes |
|---|---|---|
| `Id` | int | PK |
| `TemplateId` | int | FK → WorkoutTemplate |
| `ExerciseId` | int | FK → Exercise |
| `OrderIndex` | int | Display order within the template |
| `DefaultSets` | int | Suggested number of sets |
| `DefaultReps` | int | Suggested reps per set |

### WorkoutSession

| Field | Type | Notes |
|---|---|---|
| `Id` | int | PK |
| `UserId` | int | FK → User |
| `TemplateId` | int? | FK → WorkoutTemplate. Set to `null` when template is deleted. |
| `ScheduledDate` | DateOnly? | `null` for ad-hoc sessions |
| `IsCompleted` | bool | Default `false` |
| `CompletedAt` | DateTime? | UTC. Set when marked complete. |
| `Notes` | string | |

### SessionExercise (join table)

| Field | Type | Notes |
|---|---|---|
| `Id` | int | PK (this is `sessionExerciseId` in the API) |
| `SessionId` | int | FK → WorkoutSession |
| `ExerciseId` | int | FK → Exercise |
| `OrderIndex` | int | Display order within the session |

### Set

| Field | Type | Notes |
|---|---|---|
| `Id` | int | PK |
| `SessionExerciseId` | int | FK → SessionExercise |
| `SetNumber` | int | Caller-defined label (1, 2, 3…) |
| `WeightKg` | float | |
| `Reps` | int | |
| `IsPersonalBest` | bool | Denormalized. At most one set per exercise per user has this flag. |
| `LoggedAt` | DateTime (UTC) | Set at creation |

---

## 7. Architecture

### Layer structure

```
HTTP Request
     │
     ▼
Controller          — Thin. Extracts UserId from JWT, calls service, returns HTTP result.
     │
     ▼
Service             — Business logic. Validates ownership, orchestrates repos, maps to DTOs.
     │
     ▼
Repository          — Pure EF Core data access. No business logic.
     │
     ▼
AppDbContext        — EF Core, PostgreSQL (Npgsql).
```

### Key principles

- **Repository pattern** — all database queries live in repositories. Services never touch `AppDbContext` directly (except `AuthService` which is a thin special case).
- **Interfaces everywhere** — controllers depend on `IExerciseService`, not `ExerciseService`. Repositories depend on `IExerciseRepository`. This enables testing and swapping implementations.
- **DTOs at the boundary** — raw EF entity models are never returned to the client. Every response is mapped to a DTO in the service layer.
- **User scoping** — every repository query filters by `userId`. The `UserId` is read from the JWT claim `ClaimTypes.NameIdentifier` in each controller: `int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)`. This ensures no cross-user data leakage regardless of the ID in the URL.
- **Async throughout** — all repository and service methods are `async/await`.

---

## 8. Personal Best System

The personal best (PB) system tracks the heaviest weight ever lifted for each exercise, per user.

### Storage

There are two pieces of state:

1. **`Set.IsPersonalBest`** — A boolean flag on individual sets. At any given time, exactly zero or one set per exercise per user has this flag as `true`. This is the "heavyweight" flag — the single set that is the current record.
2. **`Exercise.PersonalBestSetId`** — A nullable FK on the exercise pointing to its current PB set. This is a denormalized read-optimization so the PB weight can be fetched without joining through sets.

### On log (`POST .../sets`)

1. Load all existing sets for this exercise (across all sessions) for the user.
2. Find the current max weight among them.
3. If the new set's `weightKg > currentMax`:
   - Set `newSet.IsPersonalBest = true`.
   - Find the previous PB set and set its `IsPersonalBest = false`.
4. Otherwise `IsPersonalBest = false`.
5. After saving, update `Exercise.PersonalBestSetId` to point to whichever set now has `IsPersonalBest = true`.

### On edit (`PUT .../sets/{setId}`)

A full recalculation is triggered:

1. Load all sets for the exercise (now including the edited set with its new weight).
2. Find the highest weight among all sets.
3. Among sets with that highest weight, pick the **earliest logged** one (`OrderBy(LoggedAt).First()`).
4. Set only that set's `IsPersonalBest = true`; set all others to `false`.
5. Update `Exercise.PersonalBestSetId`.

### On delete (`DELETE .../sets/{setId}`)

1. Check if the deleted set was the personal best.
2. If not, just delete it — nothing else to update.
3. If yes:
   - Delete the set.
   - Load all remaining sets for the exercise.
   - If any remain, run the full recalculation (same as edit).
   - Update `Exercise.PersonalBestSetId`.

### Edge cases

- If all sets for an exercise are deleted, `Exercise.PersonalBestSetId` becomes `null` and `personalBest` in the exercise DTO returns `null`.
- If two sets have equal max weight, the **earlier** one (by `LoggedAt`) is the canonical PB.
- The progress endpoint (`GET /api/exercises/{id}/progress`) returns `personalBest: 0` (not null) when no sets exist — this is different from the exercise list endpoint which returns `personalBest: null`.

---

## 9. Security & Rate Limiting

### Rate limiting

| Policy | Limit | Applies to |
|---|---|---|
| Global | 60 requests/minute | All endpoints. Partitioned by username (if authenticated) or Host header (if not). |
| `auth` | 5 requests/minute | `POST /api/auth/register` and `POST /api/auth/login` only. |

When the limit is exceeded, the server returns `429 Too Many Requests` with no body.

The auth policy overrides the global policy for the auth controller.

### CORS

Allowed origins:
- `http://localhost:3000`
- `http://localhost:5173`
- The value of the `FrontendUrl` environment variable (your production Vercel URL)

All methods and headers are allowed. Credentials (cookies, auth headers) are allowed.

### Password hashing

BCrypt (via BCrypt.Net-Next) with default work factor. Passwords are hashed on register and never stored in plaintext. Login uses `BCrypt.Verify`.

### Input validation

Data annotations on all request DTOs enforce ranges and lengths before the request reaches the controller. Invalid requests return `422` automatically.

### JWT validation

Tokens are validated for:
- Correct issuer (`Jwt:Issuer`)
- Correct audience (`Jwt:Audience`)
- Valid signature (HMAC-SHA256 with `Jwt:Key`)
- Not expired (`ValidateLifetime = true`)

---

## 10. Deployment

### Environment variables

Set these in the Render dashboard under **Environment**:

| Variable | Example value | Purpose |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Disables OpenAPI docs in prod |
| `ASPNETCORE_URLS` | `http://+:8080` | The port Render's reverse proxy connects to |
| `ConnectionStrings__DefaultConnection` | `Host=...;Database=...;Username=...;Password=...` | PostgreSQL connection string (Supabase or Render Postgres) |
| `Jwt__Key` | `a-long-random-secret-string` | JWT signing key. Use a random 64+ character string. |
| `Jwt__Issuer` | `GymTrackerApi` | JWT issuer claim |
| `Jwt__Audience` | `GymTrackerApi` | JWT audience claim |
| `FrontendUrl` | `https://your-app.vercel.app` | Added to CORS allowed origins |

> **Double underscore (`__`) in env var names** maps to colon (`:`) in .NET config. So `Jwt__Key` maps to `Jwt:Key` in `appsettings.json`.

### Vercel frontend

Add one environment variable:

| Variable | Value |
|---|---|
| `VITE_API_URL` | `https://your-service.onrender.com` |

### Auto-migration

The app runs `db.Database.Migrate()` on startup before the HTTP server begins accepting requests. This applies any pending EF Core migrations to the production database automatically on each deploy.

### Docker

The project includes a `Dockerfile` at the project root. Render uses this to build the container. The app listens on port `8080`.

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["GymBackend.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GymBackend.dll"]
```

---

## 11. Full DTO Reference

### Request DTOs

#### `RegisterRequestDto`
| Field | Type | Validation |
|---|---|---|
| `username` | string | Required. 3–50 chars. |
| `email` | string | Required. Email format. Max 256 chars. |
| `password` | string | Required. 6–100 chars. |

#### `LoginRequestDto`
| Field | Type | Validation |
|---|---|---|
| `email` | string | Required. Email format. Max 256 chars. |
| `password` | string | Required. Max 256 chars. |

#### `CreateExerciseDto`
| Field | Type | Validation |
|---|---|---|
| `name` | string | Required. Max 100 chars. |
| `muscleGroup` | string | Required. Max 100 chars. |
| `notes` | string | Optional. Max 500 chars. |

#### `UpdateExerciseDto`
| Field | Type | Validation |
|---|---|---|
| `name` | string | Required. Max 100 chars. |
| `muscleGroup` | string | Required. Max 100 chars. |
| `notes` | string | Optional. Max 500 chars. |

#### `CreateTemplateDto`
| Field | Type | Validation |
|---|---|---|
| `name` | string | Required. Max 100 chars. |
| `description` | string | Optional. Max 500 chars. |
| `dayOfWeek` | string | Optional. Max 20 chars. |

#### `UpdateTemplateDto`
| Field | Type | Validation |
|---|---|---|
| `name` | string | Required. Max 100 chars. |
| `description` | string | Optional. Max 500 chars. |
| `dayOfWeek` | string | Optional. Max 20 chars. |

#### `AddTemplateExerciseDto`
| Field | Type | Validation |
|---|---|---|
| `exerciseId` | int | Required. |
| `defaultSets` | int | 1–100. Default: `3`. |
| `defaultReps` | int | 1–100. Default: `10`. |
| `orderIndex` | int | 0–1000. Default: `0`. |

#### `ReorderExercisesDto`
| Field | Type | Validation |
|---|---|---|
| `items` | `ReorderItemDto[]` | Required. |

#### `ReorderItemDto`
| Field | Type | Validation |
|---|---|---|
| `templateExerciseId` | int | Required. |
| `orderIndex` | int | 0–1000. |

#### `CreateSessionDto`
| Field | Type | Validation |
|---|---|---|
| `templateId` | int? | Optional. |
| `scheduledDate` | string (date)? | Optional. |
| `notes` | string | Optional. Max 1000 chars. |

#### `AddSessionExerciseDto`
| Field | Type | Validation |
|---|---|---|
| `exerciseId` | int | Required. |
| `orderIndex` | int | 0–1000. Default: `0`. |

#### `LogSetDto`
| Field | Type | Validation |
|---|---|---|
| `setNumber` | int | Required. 1–100. |
| `weightKg` | float | 0.5–1000. |
| `reps` | int | 1–200. |

#### `UpdateSetDto`
| Field | Type | Validation |
|---|---|---|
| `setNumber` | int | Required. 1–100. |
| `weightKg` | float | 0.5–1000. |
| `reps` | int | 1–200. |

---

### Response DTOs

#### `AuthResponseDto`
| Field | Type |
|---|---|
| `token` | string |
| `username` | string |

#### `ExerciseDto`
| Field | Type |
|---|---|
| `id` | int |
| `name` | string |
| `muscleGroup` | string |
| `notes` | string |
| `personalBest` | float? |

#### `ExerciseProgressDto`
| Field | Type |
|---|---|
| `exerciseId` | int |
| `exerciseName` | string |
| `personalBest` | float |
| `history` | `ProgressPointDto[]` |

#### `ProgressPointDto`
| Field | Type |
|---|---|
| `date` | string (date) |
| `bestWeight` | float |
| `totalReps` | int |
| `totalSets` | int |

#### `TemplateDto`
| Field | Type |
|---|---|
| `id` | int |
| `name` | string |
| `description` | string |
| `dayOfWeek` | string |

#### `TemplateDetailDto`
| Field | Type |
|---|---|
| `id` | int |
| `name` | string |
| `description` | string |
| `dayOfWeek` | string |
| `exercises` | `TemplateExerciseDto[]` |

#### `TemplateExerciseDto`
| Field | Type |
|---|---|
| `templateExerciseId` | int |
| `exerciseId` | int |
| `exerciseName` | string |
| `muscleGroup` | string |
| `orderIndex` | int |
| `defaultSets` | int |
| `defaultReps` | int |

#### `SessionDto`
| Field | Type |
|---|---|
| `id` | int |
| `scheduledDate` | string (date)? |
| `isCompleted` | bool |
| `completedAt` | string (datetime)? |
| `notes` | string |
| `templateId` | int? |
| `templateName` | string? |

#### `SessionDetailDto` (extends `SessionDto`)
| Field | Type |
|---|---|
| *(all SessionDto fields)* | |
| `exercises` | `SessionExerciseDto[]` |

#### `SessionExerciseDto`
| Field | Type |
|---|---|
| `sessionExerciseId` | int |
| `exerciseId` | int |
| `exerciseName` | string |
| `muscleGroup` | string? |
| `orderIndex` | int |
| `sets` | `SetDto[]` |

#### `SetDto`
| Field | Type |
|---|---|
| `id` | int |
| `setNumber` | int |
| `weightKg` | float |
| `reps` | int |
| `isPersonalBest` | bool |
| `loggedAt` | string (datetime, UTC) |

#### `WeeklyDashboardDto`
| Field | Type |
|---|---|
| `weekStart` | string (date) |
| `weekEnd` | string (date) |
| `totalScheduled` | int |
| `totalCompleted` | int |
| `sessions` | `SessionDto[]` |
