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
