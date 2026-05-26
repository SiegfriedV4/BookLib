using BookLibrary.Domain;
using MediatR;

namespace BookLibrary.Application;

public record UpdateBookCommand(Guid Id, string Title, string Author) : IRequest<bool>;

public class UpdateBookCommandHandler(IBookRepository repo) : IRequestHandler<UpdateBookCommand, bool>
{
    public Task<bool> Handle(UpdateBookCommand req, CancellationToken ct) =>
        repo.UpdateAsync(req.Id, req.Title, req.Author, ct);
}
