using BookLibrary.Domain;
using MediatR;

namespace BookLibrary.Application;

public record AddBookCommand(string Title, string Author) : IRequest<Guid>;

public class AddBookCommandHandler(IBookRepository repo) : IRequestHandler<AddBookCommand, Guid>
{
    public Task<Guid> Handle(AddBookCommand req, CancellationToken ct) =>
        repo.AddAsync(req.Title, req.Author, ct);
}
