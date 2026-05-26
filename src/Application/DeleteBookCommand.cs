using BookLibrary.Domain;
using MediatR;

namespace BookLibrary.Application;

public record DeleteBookCommand(string Id) : IRequest<bool>;

public class DeleteBookCommandHandler(IBookRepository repo) : IRequestHandler<DeleteBookCommand, bool>
{
    public Task<bool> Handle(DeleteBookCommand req, CancellationToken ct) =>
        repo.DeleteAsync(req.Id, ct);
}
