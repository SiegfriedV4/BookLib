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
        // POST /api/books — create
        app.MapPost("/api/books", async (IMediator m, AddBookRequest req) =>
            Results.Ok(new { id = await m.Send(new AddBookCommand(req.Title, req.Author)) }));

        // GET /api/books — list all
        app.MapGet("/api/books", async (IMediator m) =>
            Results.Ok(await m.Send(new GetBooksQuery())));

        // GET /api/books/{id} — get one
        app.MapGet("/api/books/{id:guid}", async (IMediator m, Guid id) =>
            await m.Send(new GetBookByIdQuery(id)) is { } book
                ? Results.Ok(book)
                : Results.NotFound());

        // GET /api/books/search?term=... — search
        app.MapGet("/api/books/search", async (IMediator m, string term) =>
            Results.Ok(await m.Send(new SearchBooksQuery(term))));

        // PUT /api/books/{id} — update
        app.MapPut("/api/books/{id:guid}", async (IMediator m, Guid id, AddBookRequest req) =>
            await m.Send(new UpdateBookCommand(id, req.Title, req.Author))
                ? Results.NoContent()
                : Results.NotFound());

        // DELETE /api/books/{id} — delete
        app.MapDelete("/api/books/{id:guid}", async (IMediator m, Guid id) =>
            await m.Send(new DeleteBookCommand(id))
                ? Results.NoContent()
                : Results.NotFound());

        // Heavy queries

        // GET /api/books/paged?page=1&pageSize=50
        app.MapGet("/api/books/paged", async (IMediator m, int page, int pageSize) =>
            Results.Ok(await m.Send(new GetPagedBooksQuery(page, pageSize))));

        // GET /api/books/stats/authors — count books per author
        app.MapGet("/api/books/stats/authors", async (IMediator m) =>
            Results.Ok(await m.Send(new GetAuthorStatsQuery())));

        // GET /api/books/by-author?author=Martin Fowler
        app.MapGet("/api/books/by-author", async (IMediator m, string author) =>
            Results.Ok(await m.Send(new GetBooksByAuthorQuery(author))));

        // GET /api/books/count
        app.MapGet("/api/books/count", async (IMediator m) =>
            Results.Ok(new { count = await m.Send(new GetBookCountQuery()) }));
    }
}

record AddBookRequest(string Title, string Author);
