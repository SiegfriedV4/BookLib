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
