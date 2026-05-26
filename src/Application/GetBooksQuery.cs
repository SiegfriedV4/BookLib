using BookLibrary.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookLibrary.Application;

public record BookDto(Guid Id, string Title, string Author);
public record GetBooksQuery : IRequest<List<BookDto>>;

public class GetBooksQueryHandler(BookContext db) : IRequestHandler<GetBooksQuery, List<BookDto>>
{
    public Task<List<BookDto>> Handle(GetBooksQuery req, CancellationToken ct) =>
        db.Books.Select(b => new BookDto(b.Id, b.Title, b.Author)).ToListAsync(ct);
}
