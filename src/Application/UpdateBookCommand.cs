using BookLibrary.Infrastructure;
using MediatR;

namespace BookLibrary.Application;

public record UpdateBookCommand(Guid Id, string Title, string Author) : IRequest<bool>;

public class UpdateBookCommandHandler(BookContext db) : IRequestHandler<UpdateBookCommand, bool>
{
    public async Task<bool> Handle(UpdateBookCommand req, CancellationToken ct)
    {
        var book = await db.Books.FindAsync([req.Id], ct);
        if (book is null) return false;

        book.Title = req.Title;
        book.Author = req.Author;
        await db.SaveChangesAsync(ct);
        return true;
    }
}
