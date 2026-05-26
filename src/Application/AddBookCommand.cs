using BookLibrary.Domain;
using MediatR;

namespace BookLibrary.Application;

public record AddBookCommand(string Title, string Author) : IRequest<string>;

public class AddBookCommandHandler(IBookRepository repo) : IRequestHandler<AddBookCommand, string>
{
    public Task<string> Handle(AddBookCommand req, CancellationToken ct) =>
        repo.AddAsync(req.Title, req.Author, ct);
}
