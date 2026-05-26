using BookLibrary.Domain;
using MediatR;

namespace BookLibrary.Application;

public record BookDto(Guid Id, string Title, string Author);
public record GetBooksQuery : IRequest<List<BookDto>>;

public class GetBooksQueryHandler(IBookRepository repo) : IRequestHandler<GetBooksQuery, List<BookDto>>
{
    public Task<List<BookDto>> Handle(GetBooksQuery req, CancellationToken ct) =>
        repo.GetAllAsync(ct);
}
