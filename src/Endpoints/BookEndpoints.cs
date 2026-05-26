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
        app.MapGet("/api/books/{id}", async (IMediator m, string id) =>
            await m.Send(new GetBookByIdQuery(id)) is { } book
                ? Results.Ok(book)
                : Results.NotFound());

        // GET /api/books/search?term=... — search
        app.MapGet("/api/books/search", async (IMediator m, string term) =>
            Results.Ok(await m.Send(new SearchBooksQuery(term))));

        // PUT /api/books/{id} — update
        app.MapPut("/api/books/{id}", async (IMediator m, string id, AddBookRequest req) =>
            await m.Send(new UpdateBookCommand(id, req.Title, req.Author))
                ? Results.NoContent()
                : Results.NotFound());

        // DELETE /api/books/{id} — delete
        app.MapDelete("/api/books/{id}", async (IMediator m, string id) =>
            await m.Send(new DeleteBookCommand(id))
                ? Results.NoContent()
                : Results.NotFound());
    }
}

record AddBookRequest(string Title, string Author);
