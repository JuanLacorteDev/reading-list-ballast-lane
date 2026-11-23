# CLAUDE.md

This file provides guidance to Claude Code when working in this repository.

---
## Development Plan

Maintain a `GENAI.md` file in the repository root to document relevant development decisions and AI-assisted interactions that require my review or intervention.

For each entry, briefly describe:

- **Context:** The problem, requirement, or 
decision that came up.
- **My input:** The clarification, instructions, or decision I provided.
- **Resolution:** How the situation was addressed and what was implemented.
- **Outcome:** The result and how it was validated, when applicable.

Focus on meaningful situations, such as architectural decisions, changes in requirements, corrections to AI-generated suggestions, and technical trade-offs that required my involvement.

Do not document routine implementation steps or every prompt and response. Keep the records concise and factual, and update `GENAI.md` as development progresses.

# Project Overview

Task Manager Ballastlane (MVP) — a small, maintainable RESTful API for managing user tasks.

## Goal

Allow users to create, view, update, and delete their tasks while ensuring that each user can access only their own tasks.

## User Story

As a user who needs to keep track of tasks and deadlines,
I want to create and manage my tasks,
so that I always know what needs to be done and when it is due.

## Scope

Implement a RESTful API that supports task CRUD operations.

The API also needs a minimal user-creation endpoint for development and API testing. Full user registration, password management, login, and access-token generation are out of scope for this MVP.

## Technology Stack

- .NET 10 / ASP.NET Core Web API
- Clean Architecture: Domain / Application / Infrastructure / API
- SQLite
- ADO.NET
- Automated tests (xUnit)
- Do not use Entity Framework Core; database access must use ADO.NET.

## Domain Model

### User

- `id`: unique identifier (GUID)
- `name`: string

### Task

- `id`: unique identifier (GUID)
- `user_id`: identifier of the task owner (GUID)
- `title`: required string
- `description`: optional string
- `status`: one of `Pending`, `InProgress`, or `Completed`
- `due_date`: date-only value serialized as `YYYY-MM-DD`

Use `DateOnly` for `due_date`. Do not treat a due date as a timestamp, and do not assume that a due date must be in the future. Use snake_case JSON property names (including `due_date`).
Persist the status as a numeric enum value in SQLite, using explicit and stable numeric assignments to avoid breaking existing records when status names change.
When the API return a task should return the text of status instead of numeric enum value.

## API Endpoints

### Users

- `POST /api/users` — create a minimal user and return the generated user ID. This endpoint exists only to support development and API testing in this MVP.

### Tasks

All task endpoints must operate in the context of the current user, identified by the `X-User-Id` request header.

- `POST /api/tasks` — create a task for the current user
- `GET /api/tasks` — list only the current user's tasks
- `GET /api/tasks/{id}` — get one task belonging to the current user
- `PUT /api/tasks/{id}` — update one task belonging to the current user
- `DELETE /api/tasks/{id}` — delete one task belonging to the current user

The API must derive the task owner from the current-user context. Do not accept `user_id` from the task creation or update request body.

## Current User Context (Temporary MVP Approach)

Real authentication is not required in this first version. For development and API testing, the client will send the user ID through the `X-User-Id` request header.

Implement this behind an abstraction such as `ICurrentUserContext`:

- The API layer is responsible for reading and validating the `X-User-Id` header.
- The Application and Domain layers must not depend on `HttpContext` or ASP.NET Core types.
- Application use cases must obtain the current user through the abstraction, not by reading HTTP headers directly.
- Keep the implementation replaceable so a future JWT authentication mechanism can provide the current user ID without changing the use cases.
- Reject a missing or malformed user ID with a clear client error.
- Ensure the referenced user exists before creating or querying tasks.

Important: `X-User-Id` is a temporary development/testing mechanism, not authentication.

## Backend Architecture (Strict)

Follow Clean Architecture with clear dependency direction:

- **Domain:** entities, value objects (when useful), and business rules only. No references to ASP.NET Core, SQLite, ADO.NET, or other infrastructure concerns.
- **Application:** use cases, application-level validation/orchestration, and interfaces required by use cases (for example, repositories and current-user context).
- **Infrastructure:** SQLite database setup and ADO.NET repository implementations.
- **API:** controllers, request/response DTOs, HTTP concerns, dependency-injection configuration, and mapping of application results to HTTP responses. Controllers must not contain business logic.


## Engineering Practices

### Test-Driven Development

Use the Red-Green-Refactor cycle:

1. Write a focused test for the behavior or rule.
2. Run it and confirm that it fails for the expected reason.
3. Implement the smallest change that makes it pass.
4. Refactor while keeping the tests green.

Do not claim that a test passed unless you actually ran it.

### Database and SQL

- Use ADO.NET through `Microsoft.Data.Sqlite`.
- Use SQL parameters for every value originating from a request or other external input. (preventing SQL Injection)
- Enable SQLite foreign-key enforcement in the appsettings.json connection string (`Foreign Keys=True`).
- Ensure a task references an existing user.
- Keep SQL and SQLite-specific details inside Infrastructure.

### Validation and Ownership Rules

- `title` is required and must not be blank.
- `description` is optional.
- `status` must be one of the supported values: `Pending`, `InProgress`, or `Completed`.
- `due_date` must be a valid date in `YYYY-MM-DD` format.
- A user must never be able to list, retrieve, update, or delete another user's tasks.
- Scope database queries for individual tasks by both task ID and current user ID.
- For a task that does not exist or does not belong to the current user, return `404 Not Found`. Do not disclose whether a task belonging to another user exists.

## HTTP Behavior

Use appropriate HTTP status codes:

- `201 Created` when a user or task is created.
- `200 OK` when a task is retrieved, listed, or updated successfully.
- `204 No Content` when a task is deleted successfully.
- `404 Not Found` when the user identified by `X-User-Id` does not exist.
- `400 Bad Request` for malformed headers, invalid payloads, or validation errors.
- `404 Not Found` when the requested task is missing or is not owned by the current user.

Use a consistent error response format. Result Pattern for expected bussiness fails and Exception for invariants that should not happen.

## Tests and Validation

At minimum, add automated tests covering:

- Successful task creation, listing, retrieval, update, and deletion.
- Required-field and invalid-value validation.
- Invalid date formats and `DateOnly` serialization.
- Missing and malformed `X-User-Id` headers.
- A user attempting to access, update, or delete another user's task.
- Persistence and user/task relationship in SQLite.
- SQL-injection-like input being treated as data, not executable SQL.

Prefer integration tests against SQLite in addition to focused unit tests for domain and application behavior.



## Error handling (follow exactly, do not redesign)

Use three mechanisms, each with one job:

1. **Broken business rule → throw `DomainException`** (Domain layer only).
   Entities and value objects validate in factory methods / behavior methods and throw.
   A global `IExceptionHandler` (`DomainExceptionHandler`) maps it to HTTP 400 ProblemDetails.
   Services never try/catch it.

2. **Expected use-case outcome (not found, conflict, invalid credentials) → return a `Result`.**
   Use exactly these types in `Application/Common/Results`:

   ```csharp
   public enum ErrorType { Validation, NotFound, Conflict, Unauthorized }

   public sealed record Error(string Code, string Message, ErrorType Type)
   {
       public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);
       public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
       public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
       public static Error Unauthorized(string code, string message) => new(code, message, ErrorType.Unauthorized);
   }

   // Result: IsSuccess, IsFailure, Error?; Success() / Failure(error)
   // Result<T> : Result with Value (throws if failed);
   // implicit conversions from T and from Error, so services can `return book;` or `return TaskErrors.NotFound;`
   ```

   Errors are declared once per feature as static fields, e.g.:

   ```csharp
   public static class TaskErrors
   {
       public static readonly Error NotFound = Error.NotFound("Task.NotFound", "Task was not found.");
   }
   ```

3. **Anything else → unhandled exception → 500** via `UseExceptionHandler`, no internal details in the response.

### Mapping to HTTP (one place only)

- `ErrorStatusCodes.From(ErrorType)`: Validation 400, Unauthorized 401, NotFound 404, Conflict 409.
- An abstract `ApiControllerBase : ControllerBase` with `[ApiController]` exposes
  `protected IActionResult Problem(Error error)` that returns RFC 9457 ProblemDetails with
  `title = error.Code`, `detail = error.Message`, `status` from `ErrorStatusCodes`, and an extension `code = error.Code`.
- Controllers stay thin: `return result.IsSuccess ? Ok(result.Value) : Problem(result.Error!);`

### Rules

- A task that exists but belongs to another user returns `TaskErrors.NotFound` (404), never 403, to avoid id enumeration.
- Do NOT create one exception class per error, do NOT add OneOf / ErrorOr / FluentResults or any other library,
  do NOT add more ErrorType values than the four above, do NOT put HTTP status codes in the Application layer.
- Add a unit test that fails if a new `ErrorType` value is added without a mapping in `ErrorStatusCodes`.
