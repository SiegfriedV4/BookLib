# CQRS Book Library — Super Simple

**One entity. Six endpoints. Pure CQRS with EF Core + MediatR.**

---

## What We're Building & Why

This exercise teaches two patterns working together:

- **CQRS** (Command Query Responsibility Segregation) — split code that *writes* data from code that *reads* data. Why? Reads and writes have different concerns; keeping them separate makes each simpler and easier to change.
- **EF Core** — maps C# objects to a database without writing SQL. Why? It removes boilerplate and lets you focus on the domain logic.
- **MediatR** — a mediator library that routes commands/queries to their handlers. Why? It decouples the API layer from the application logic — your endpoints don't know or care how a command is handled.

---

## Project Structure

```
src/
├── Domain/          → The Book entity (pure C#, no dependencies)
├── Infrastructure/  → EF Core DbContext (database access)
├── Application/     → Commands, Queries, Handlers (the business logic)
├── Endpoints/       → API route definitions (maps HTTP → MediatR)
└── Api/             → Program.cs (wires everything together)
```

> **Why these layers?**
> Each layer has one job. Domain knows nothing about the database. Application knows nothing about HTTP. Endpoints know nothing about EF Core. This means you can change any layer without touching the others.

---

## Setup

```bash
dotnet new sln -n BookLibrary
dotnet new classlib -n BookLibrary.Domain         -o src/Domain
dotnet new classlib -n BookLibrary.Application    -o src/Application
dotnet new classlib -n BookLibrary.Infrastructure -o src/Infrastructure
dotnet new classlib -n BookLibrary.Endpoints      -o src/Endpoints
dotnet new webapi   -n BookLibrary.Api            -o src/Api
dotnet sln add src/**/*.csproj

# Packages
dotnet add src/Infrastructure package Microsoft.EntityFrameworkCore.InMemory
dotnet add src/Application    package MediatR
dotnet add src/Application    package Microsoft.EntityFrameworkCore
dotnet add src/Endpoints      package MediatR
dotnet add src/Api            package MediatR
dotnet add src/Api            package Swashbuckle.AspNetCore

# Project references
dotnet add src/Application reference src/Domain
dotnet add src/Application reference src/Infrastructure
dotnet add src/Infrastructure reference src/Domain
dotnet add src/Endpoints   reference src/Application
dotnet add src/Api         reference src/Application
dotnet add src/Api         reference src/Infrastructure
dotnet add src/Api         reference src/Endpoints

# Endpoints needs ASP.NET Core types — add to its .csproj:
# <FrameworkReference Include="Microsoft.AspNetCore.App" />
```

> **Why InMemory database?** No SQL Server setup needed for learning. Swap it for SQL Server later with one line change in `Program.cs`.

---

## Domain — `src/Domain/Book.cs`

```csharp
namespace BookLibrary.Domain;

public class Book
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
}
```

> **Why a separate Domain project?** The entity is pure C# with zero dependencies. No EF, no MediatR, no HTTP. This is the heart of the app and it stays clean.

---

## Infrastructure — `src/Infrastructure/BookContext.cs`

```csharp
using BookLibrary.Domain;
using Microsoft.EntityFrameworkCore;

namespace BookLibrary.Infrastructure;

public class BookContext(DbContextOptions<BookContext> options) : DbContext(options)
{
    public DbSet<Book> Books { get; set; } = null!;
}
```

> **Why DbContext in its own project?** Infrastructure is the only place that knows about EF Core. If you switch from EF Core to Dapper or a different ORM, you only touch this project.

---

## Application — Commands & Queries

### Why Commands and Queries?

A **Command** changes state — it writes to the database and returns a result (like the new ID).  
A **Query** reads state — it never modifies anything, just returns data.

Keeping them separate means your read paths stay simple (no business rules leaking in) and your write paths stay focused (no accidental reads mixed in).

**`AddBookCommand.cs`**
```csharp
using BookLibrary.Domain;
using BookLibrary.Infrastructure;
using MediatR;

namespace BookLibrary.Application;

// IRequest<Guid> = "this command returns a Guid"
public record AddBookCommand(string Title, string Author) : IRequest<Guid>;

public class AddBookCommandHandler(BookContext db) : IRequestHandler<AddBookCommand, Guid>
{
    public async Task<Guid> Handle(AddBookCommand req, CancellationToken ct)
    {
        var book = new Book { Title = req.Title, Author = req.Author };
        db.Books.Add(book);
        await db.SaveChangesAsync(ct);
        return book.Id;
    }
}
```

**`GetBooksQuery.cs`**
```csharp
using BookLibrary.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookLibrary.Application;

public record BookDto(Guid Id, string Title, string Author);

// IRequest<List<BookDto>> = "this query returns a list"
public record GetBooksQuery : IRequest<List<BookDto>>;

public class GetBooksQueryHandler(BookContext db) : IRequestHandler<GetBooksQuery, List<BookDto>>
{
    public Task<List<BookDto>> Handle(GetBooksQuery req, CancellationToken ct) =>
        db.Books.Select(b => new BookDto(b.Id, b.Title, b.Author)).ToListAsync(ct);
}
```

**`GetBookByIdQuery.cs`**
```csharp
using BookLibrary.Infrastructure;
using MediatR;

namespace BookLibrary.Application;

public record GetBookByIdQuery(Guid Id) : IRequest<BookDto?>;

public class GetBookByIdQueryHandler(BookContext db) : IRequestHandler<GetBookByIdQuery, BookDto?>
{
    public async Task<BookDto?> Handle(GetBookByIdQuery req, CancellationToken ct)
    {
        var book = await db.Books.FindAsync([req.Id], ct);
        return book is null ? null : new BookDto(book.Id, book.Title, book.Author);
    }
}
```

**`SearchBooksQuery.cs`**
```csharp
using BookLibrary.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookLibrary.Application;

public record SearchBooksQuery(string Term) : IRequest<List<BookDto>>;

public class SearchBooksQueryHandler(BookContext db) : IRequestHandler<SearchBooksQuery, List<BookDto>>
{
    public Task<List<BookDto>> Handle(SearchBooksQuery req, CancellationToken ct)
    {
        var term = req.Term.ToLower();
        return db.Books
            .Where(b => b.Title.ToLower().Contains(term) || b.Author.ToLower().Contains(term))
            .Select(b => new BookDto(b.Id, b.Title, b.Author))
            .ToListAsync(ct);
    }
}
```

**`UpdateBookCommand.cs`**
```csharp
using BookLibrary.Infrastructure;
using MediatR;

namespace BookLibrary.Application;

public record UpdateBookCommand(Guid Id, string Title, string Author) : IRequest<bool>;

public class UpdateBookCommandHandler(BookContext db) : IRequestHandler<UpdateBookCommand, bool>
{
    public async Task<bool> Handle(UpdateBookCommand req, CancellationToken ct)
    {
        var book = await db.Books.FindAsync([req.Id], ct);
        if (book is null) return false;

        book.Title = req.Title;
        book.Author = req.Author;
        await db.SaveChangesAsync(ct);
        return true;
    }
}
```

**`DeleteBookCommand.cs`**
```csharp
using BookLibrary.Infrastructure;
using MediatR;

namespace BookLibrary.Application;

public record DeleteBookCommand(Guid Id) : IRequest<bool>;

public class DeleteBookCommandHandler(BookContext db) : IRequestHandler<DeleteBookCommand, bool>
{
    public async Task<bool> Handle(DeleteBookCommand req, CancellationToken ct)
    {
        var book = await db.Books.FindAsync([req.Id], ct);
        if (book is null) return false;

        db.Books.Remove(book);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
```

> **Why MediatR?** Instead of the endpoint calling the handler directly, it sends a message (`mediator.Send(command)`). MediatR finds the right handler automatically. This means your endpoints have zero knowledge of how commands are handled — easy to test, easy to swap out.

---

## Endpoints — `src/Endpoints/BookEndpoints.cs`

```csharp
using BookLibrary.Application;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BookLibrary.Endpoints;

public static class BookEndpoints
{
    public static void MapBookEndpoints(this WebApplication app)
    {
        app.MapPost("/api/books", async (IMediator m, AddBookRequest req) =>
            Results.Ok(new { id = await m.Send(new AddBookCommand(req.Title, req.Author)) }));

        app.MapGet("/api/books", async (IMediator m) =>
            Results.Ok(await m.Send(new GetBooksQuery())));

        app.MapGet("/api/books/{id:guid}", async (IMediator m, Guid id) =>
            await m.Send(new GetBookByIdQuery(id)) is { } book
                ? Results.Ok(book)
                : Results.NotFound());

        app.MapGet("/api/books/search", async (IMediator m, string term) =>
            Results.Ok(await m.Send(new SearchBooksQuery(term))));

        app.MapPut("/api/books/{id:guid}", async (IMediator m, Guid id, AddBookRequest req) =>
            await m.Send(new UpdateBookCommand(id, req.Title, req.Author))
                ? Results.NoContent()
                : Results.NotFound());

        app.MapDelete("/api/books/{id:guid}", async (IMediator m, Guid id) =>
            await m.Send(new DeleteBookCommand(id))
                ? Results.NoContent()
                : Results.NotFound());
    }
}

record AddBookRequest(string Title, string Author);
```

> **Why a separate Endpoints project?** Endpoints only know how to translate HTTP requests into MediatR messages. No EF Core, no domain logic. Keeping them separate means `Program.cs` stays clean — one call to `MapBookEndpoints()` is all it needs.

---

## API — `src/Api/Program.cs`

```csharp
using BookLibrary.Endpoints;
using BookLibrary.Application;
using BookLibrary.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BookContext>(o => o.UseInMemoryDatabase("BookLibrary"));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<AddBookCommand>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapBookEndpoints();

app.Run();
```

> **Why is Program.cs so short?** Because each layer handles its own concern. `Program.cs` only wires services together — it doesn't contain routes, business logic, or database queries.

---

## Run & Test

```bash
cd src/Api && dotnet run
# Open http://localhost:5202/swagger
```

| Method | Endpoint | Command / Query |
|---|---|---|
| `POST` | `/api/books` | `AddBookCommand` |
| `GET` | `/api/books` | `GetBooksQuery` |
| `GET` | `/api/books/{id}` | `GetBookByIdQuery` |
| `GET` | `/api/books/search?term=...` | `SearchBooksQuery` |
| `PUT` | `/api/books/{id}` | `UpdateBookCommand` |
| `DELETE` | `/api/books/{id}` | `DeleteBookCommand` |

---

## Key Takeaways

| Concept | What it does | Why it matters |
|---|---|---|
| **Command** | Writes / changes data | Focused, testable, no read logic mixed in |
| **Query** | Reads data only | Never has side effects |
| **Handler** | Does the actual work | One handler per command/query — single responsibility |
| **MediatR** | Routes message → handler | Decouples HTTP layer from application logic |
| **EF Core** | Maps C# ↔ database | No SQL boilerplate; swap database with one config line |
| **Layered structure** | Each project has one job | Change one layer without breaking the others |

---

## Feature: Persisting Data with SQLite

Right now data lives in memory — it's gone the moment you stop the app. This section upgrades to **SQLite**, a file-based database. Data survives restarts and no server setup is needed.

> **Why SQLite first?** It's a single `.db` file on disk. No installation, no connection strings with passwords. Perfect for learning before moving to SQL Server or Postgres.

### What changes

Because EF Core is isolated in `Infrastructure`, swapping the database touches **one line in `Program.cs`** and adds migrations. Nothing else changes — all commands, queries, handlers and endpoints stay identical. That's the payoff of the layered architecture.

---

### Step 1: Add the SQLite package

```bash
dotnet add src/Infrastructure package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/Api            package Microsoft.EntityFrameworkCore.Design
```

> `Microsoft.EntityFrameworkCore.Design` is needed by the `dotnet ef` CLI tool to generate migrations. It only runs at design time, not in production.

---

### Step 2: Swap the provider in `Program.cs`

**`src/Api/Program.cs`** — change one line:

```csharp
// Before (in-memory, data lost on restart)
builder.Services.AddDbContext<BookContext>(o => o.UseInMemoryDatabase("BookLibrary"));

// After (SQLite, data saved to a file)
builder.Services.AddDbContext<BookContext>(o => o.UseSqlite("Data Source=books.db"));
```

Add the using at the top:
```csharp
using Microsoft.EntityFrameworkCore;
```

> `books.db` is the file that will be created in your `src/Api` folder. You can rename it anything you like.

---

### Step 3: Add a migration

A migration is EF Core generating the SQL to create your database schema (`CREATE TABLE Books...`). You run this once — and again every time you change the `Book` entity.

```bash
dotnet ef migrations add InitialCreate \
  --project src/Infrastructure \
  --startup-project src/Api
```

This creates a `Migrations/` folder inside `src/Infrastructure` with the generated SQL schema.

> **Why `--project` and `--startup-project`?** The migration lives in `Infrastructure` (where `BookContext` is), but the app needs to start from `Api` so EF Core can read your `Program.cs` configuration.

---

### Step 4: Apply the migration

```bash
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/Api
```

This creates `books.db` in `src/Api` and runs the migration, creating the `Books` table.

---

### Step 5: Run the app

```bash
cd src/Api && dotnet run
```

Open **http://localhost:5202/swagger**, add some books via `POST /api/books`, then stop the app with `Ctrl+C` and run it again — your books will still be there.

---

### What the migration generates (for reference)

```sql
CREATE TABLE "Books" (
    "Id"     TEXT NOT NULL CONSTRAINT "PK_Books" PRIMARY KEY,
    "Title"  TEXT NOT NULL,
    "Author" TEXT NOT NULL
);
```

EF Core wrote this for you based on the `Book` entity and `BookContext`.

---

### Adding a new field later

If you add a property to `Book` (e.g. `PublishedYear`):

```csharp
// src/Domain/Book.cs
public int PublishedYear { get; set; }
```

Run a new migration:
```bash
dotnet ef migrations add AddPublishedYear \
  --project src/Infrastructure \
  --startup-project src/Api

dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/Api
```

EF Core generates:
```sql
ALTER TABLE "Books" ADD COLUMN "PublishedYear" INTEGER NOT NULL DEFAULT 0;
```

> Each migration stacks on top of the previous one. EF Core tracks which migrations have been applied so it never runs the same one twice.

---

### SQLite vs other databases

When you outgrow SQLite, swap the provider again — one line change:

| Database | Package | Connection string |
|---|---|---|
| **SQLite** | `EntityFrameworkCore.Sqlite` | `Data Source=books.db` |
| **SQL Server** | `EntityFrameworkCore.SqlServer` | `Server=...;Database=BookLibrary` |
| **PostgreSQL** | `Npgsql.EntityFrameworkCore.PostgreSQL` | `Host=...;Database=booklibrary` |

The commands, queries, handlers and endpoints never change — only `Program.cs` and the package.
