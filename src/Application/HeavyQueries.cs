using BookLibrary.Domain;
using MediatR;

namespace BookLibrary.Application;

// Paginated list
public record GetPagedBooksQuery(int Page, int PageSize) : IRequest<PagedBooksDto>;

public class GetPagedBooksQueryHandler(IBookRepository repo) : IRequestHandler<GetPagedBooksQuery, PagedBooksDto>
{
    public Task<PagedBooksDto> Handle(GetPagedBooksQuery req, CancellationToken ct) =>
        repo.GetPagedAsync(req.Page, req.PageSize, ct);
}

// Author stats (aggregation)
public record GetAuthorStatsQuery : IRequest<List<AuthorStatsDto>>;

public class GetAuthorStatsQueryHandler(IBookRepository repo) : IRequestHandler<GetAuthorStatsQuery, List<AuthorStatsDto>>
{
    public Task<List<AuthorStatsDto>> Handle(GetAuthorStatsQuery req, CancellationToken ct) =>
        repo.GetAuthorStatsAsync(ct);
}

// Books by author
public record GetBooksByAuthorQuery(string Author) : IRequest<List<BookDto>>;

public class GetBooksByAuthorQueryHandler(IBookRepository repo) : IRequestHandler<GetBooksByAuthorQuery, List<BookDto>>
{
    public Task<List<BookDto>> Handle(GetBooksByAuthorQuery req, CancellationToken ct) =>
        repo.GetBooksByAuthorAsync(req.Author, ct);
}

// Total count
public record GetBookCountQuery : IRequest<int>;

public class GetBookCountQueryHandler(IBookRepository repo) : IRequestHandler<GetBookCountQuery, int>
{
    public Task<int> Handle(GetBookCountQuery req, CancellationToken ct) =>
        repo.CountAsync(ct);
}
