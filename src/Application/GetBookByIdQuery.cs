using BookLibrary.Domain;
using MediatR;

namespace BookLibrary.Application;

public record GetBookByIdQuery(string Id) : IRequest<BookDto?>;

public class GetBookByIdQueryHandler(IBookRepository repo) : IRequestHandler<GetBookByIdQuery, BookDto?>
{
    public Task<BookDto?> Handle(GetBookByIdQuery req, CancellationToken ct) =>
        repo.GetByIdAsync(req.Id, ct);
}
