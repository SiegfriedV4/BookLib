using BookLibrary.Domain;
using MediatR;

namespace BookLibrary.Application;

public record SearchBooksQuery(string Term) : IRequest<List<BookDto>>;

public class SearchBooksQueryHandler(IBookRepository repo) : IRequestHandler<SearchBooksQuery, List<BookDto>>
{
    public Task<List<BookDto>> Handle(SearchBooksQuery req, CancellationToken ct) =>
        repo.SearchAsync(req.Term, ct);
}
