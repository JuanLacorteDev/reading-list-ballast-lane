# GENAI.md — AI-assisted development log

## Notes

## Notes

The initial draft of `CLAUDE.md` was written by me and later
refined with GPT.

I also refined the error-handling description with GPT's assistance,
then documented the requirements in `CLAUDE.md` to give Claude
Code clear implementation guidance.

## 1. Error-handling design (DomainException + Result)

- **Context:** The first Domain draft guarded rules with `ArgumentException`. A second AI proposal had application services catch `DomainException` and convert it into a `Result` carrying a list of errors (with field names, implicit aggregation and a `FromDomain` helper).
- **My input:** First: introduce a dedicated `DomainException` for broken business rules. Then: the proposed error handling was too complex. I added an exact "Error handling" section to `CLAUDE.md`: `DomainException` is thrown by the Domain only and handled by a global `DomainExceptionHandler` (400), services never catch it. Expected outcomes return a `Result` with a single `Error` record (`ErrorType` = Validation/NotFound/Conflict/Unauthorized), errors declared once per feature (`TaskErrors`, `UserErrors`). Unhandled exceptions → 500. HTTP mapping lives in one place (`ErrorStatusCodes`, `ApiControllerBase.Problem(Error)`).
- **Resolution:** Removed the list-based Result and the try/catch in use cases. Implemented `Application/Common/Results` (`Error`, `Result`, `Result<T>` with implicit conversions) exactly as specified; `DomainException(code, message)` with codes like `Task.TitleRequired`. Input-format problems (unknown status, malformed due date) are expected outcomes → `TaskErrors.InvalidStatus` / `InvalidDueDate` (Validation).
- **Outcome:** Domain + Application unit tests (54) passing; tests assert `DomainException` propagates from use cases and that expected failures return the declared `TaskErrors` / `UserErrors`.

## 2. API-layer decisions (open for review)

- **Context:** Implementing the HTTP layer on top of the error-handling spec.
- **Resolution:**
  - `X-User-Id` is validated by a `[RequireUserIdHeader]` action filter on `TasksController`, ordered to run *before* body validation. Missing → `400 UserContext.MissingUserId`; non-GUID or empty GUID → `400 UserContext.InvalidUserId`. `HeaderCurrentUserContext` (implements `ICurrentUserContext`) only exposes the already-validated id; swapping it for a JWT/claims implementation requires no use-case changes.
  - Malformed/unreadable JSON bodies (invalid JSON, wrong JSON types, empty body) return the same ProblemDetails shape with code `Request.Invalid` (customized `InvalidModelStateResponseFactory`).
  - Request DTOs never contain `user_id`; a `user_id` sent in the body is ignored (covered by a test).
  - Task responses don't expose `user_id`; `status` is returned as its name, `due_date` as `YYYY-MM-DD`.
  - `ErrorProblemDetails` builds every error body (`title = code`, `detail`, `status`, `code`), used by `ApiControllerBase.Problem(Error)`, the header filter and `DomainExceptionHandler`; status codes come only from `ErrorStatusCodes`.
  - The `ErrorStatusCodes` coverage test lives in `TaskManager.UnitTests`, which therefore references the API project.
  - Application and Infrastructure reference `Microsoft.Extensions.DependencyInjection.Abstractions` for their `AddApplication()` / `AddInfrastructure()` extensions (no ASP.NET Core dependency).
- **Outcome:** 59 unit + 63 integration tests passing (HTTP end-to-end against a temporary SQLite file).

## 3. Smaller decisions taken by default (open for review)

- **Context:** The spec leaves some details open.
- **Resolution:**
  - Status stored as `Pending = 1`, `InProgress = 2`, `Completed = 3` (starting at 1 so a defaulted `0` is never mistaken for a valid status); a unit test pins these values.
  - Entity named `TaskItem` / `TaskItemStatus` to avoid clashing with `System.Threading.Tasks.Task` / `TaskStatus`.
  - Status names accepted case-insensitively; numeric values (`"1"`) are rejected. Status is optional on create (defaults to `Pending`) and required on update (PUT is a full replacement). `due_date` is required on both.
  - Title and description are trimmed; a whitespace-only description is stored as `null`.
  - Only the first failing input is reported (single `Error` per `Result`, per the error-handling spec).
  - NuGet packages pinned to the 10.0.x line (`Microsoft.Data.Sqlite` and `Microsoft.AspNetCore.Mvc.Testing` 10.0.15) because 11.0 packages don't support `net10.0`.
