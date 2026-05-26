using BookLibrary.Domain;
using BookLibrary.Infrastructure;
using MediatR;

namespace BookLibrary.Application;

public record AddBookCommand(string Title, string Author) : IRequest<Guid>;

public class AddBookCommandHandler(BookContext db) : IRequestHandler<AddBookCommand, Guid>
{
    public async Task<Guid> Handle(AddBookCommand req, CancellationToken ct)
    {
        var book = new Book { Title = req.Title, Author = req.Author };
        db.Books.Add(book);
        await db.SaveChangesAsync(ct);
        return book.Id;
    }
}
