using BookLibrary.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookLibrary.Application;

public record PagedBooksDto(List<BookDto> Books, int TotalCount, int Page, int PageSize);
public record AuthorStatsDto(string Author, int BookCount);

// Paginated list
public record GetPagedBooksQuery(int Page, int PageSize) : IRequest<PagedBooksDto>;

public class GetPagedBooksQueryHandler(BookContext db) : IRequestHandler<GetPagedBooksQuery, PagedBooksDto>
{
    public async Task<PagedBooksDto> Handle(GetPagedBooksQuery req, CancellationToken ct)
    {
        var total = await db.Books.CountAsync(ct);
        var books = await db.Books
            .OrderBy(b => b.Title)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(b => new BookDto(b.Id, b.Title, b.Author))
            .ToListAsync(ct);

        return new PagedBooksDto(books, total, req.Page, req.PageSize);
    }
}

// Author stats (aggregation)
public record GetAuthorStatsQuery : IRequest<List<AuthorStatsDto>>;

public class GetAuthorStatsQueryHandler(BookContext db) : IRequestHandler<GetAuthorStatsQuery, List<AuthorStatsDto>>
{
    public Task<List<AuthorStatsDto>> Handle(GetAuthorStatsQuery req, CancellationToken ct) =>
        db.Books
            .GroupBy(b => b.Author)
            .Select(g => new AuthorStatsDto(g.Key, g.Count()))
            .OrderByDescending(a => a.BookCount)
            .ToListAsync(ct);
}

// Books by author
public record GetBooksByAuthorQuery(string Author) : IRequest<List<BookDto>>;

public class GetBooksByAuthorQueryHandler(BookContext db) : IRequestHandler<GetBooksByAuthorQuery, List<BookDto>>
{
    public Task<List<BookDto>> Handle(GetBooksByAuthorQuery req, CancellationToken ct) =>
        db.Books
            .Where(b => b.Author == req.Author)
            .OrderBy(b => b.Title)
            .Select(b => new BookDto(b.Id, b.Title, b.Author))
            .ToListAsync(ct);
}

// Total count
public record GetBookCountQuery : IRequest<int>;

public class GetBookCountQueryHandler(BookContext db) : IRequestHandler<GetBookCountQuery, int>
{
    public Task<int> Handle(GetBookCountQuery req, CancellationToken ct) =>
        db.Books.CountAsync(ct);
}
