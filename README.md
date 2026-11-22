# Reading List

A small full-stack app to keep track of the books you want to read, are reading, have read or gave up on.
Built for the .NET technical interview exercise: two ASP.NET Core Web APIs (.NET 10) following Clean Architecture,
raw ADO.NET over SQLite (no Entity Framework, Dapper or Mediator), and an Angular 20 frontend.

## User story

> **As a** reader who loses track of book recommendations,
> **I want** a private list where I add books, mark how far i am with each one and rate the ones i finished,
> **so that** I always know what to read next and what i thought of what i already read.

Acceptance criteria:

1. I can create an account and sign in with email and password.
2. I only ever see and change my own books.
3. A new book needs a title and an author, and starts as **Want to read** with no rating.
4. I can move a book between **Want to read**, **Reading**, **Read** and **Abandoned**.
5. I can rate a book from 1 to 5, but only once it is **Read**. Moving it out of Read clears the rating.
6. I can filter my list by status, edit a book and delete it.
7. The app works on a phone as well as on a desktop.

## Quick start

### Prerequisites

| Tool | Version |
| --- | --- |
| .NET SDK | 10.0.100 or later (pinned in `backend/global.json`) |
| Node.js | 20.19+ or 22.12+ (required by Angular 20) |

### Run it

Open three terminals from the repository root:

```bash
# 1. Identity API  -> http://localhost:5001
dotnet run --project backend/src/ReadingList.Identity.Api

# 2. Books API     -> http://localhost:5002
dotnet run --project backend/src/ReadingList.Books.Api

# 3. Frontend      -> http://localhost:4200
cd frontend
npm ci
npm start
```

Open <http://localhost:4200> and sign in with the demo account (the login page has a **Use demo account** button):

The database file is created and seeded automatically on first start (5 books in different statuses).
Both APIs create the schema and run the seed, so the start order does not matter.

### Explore the APIs

In Development each API serves an OpenAPI document and an interactive reference (Scalar):

| API | OpenAPI | Reference UI |
| --- | --- | --- |
| Identity | <http://localhost:5001/openapi/v1.json> | <http://localhost:5001/scalar> |
| Books | <http://localhost:5002/openapi/v1.json> | <http://localhost:5002/scalar> |

### Run the tests

```bash
# Backend: Domain, Application, Infrastructure and API tests
cd backend
dotnet test

# Frontend: unit tests, headless
cd frontend
npm run test:ci
```

### Reset the data

Stop both APIs and delete `backend/readinglist.db`.
The next start recreates and reseeds it.



