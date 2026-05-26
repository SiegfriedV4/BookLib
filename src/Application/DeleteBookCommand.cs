using BookLibrary.Infrastructure;
using MediatR;

namespace BookLibrary.Application;

public record DeleteBookCommand(Guid Id) : IRequest<bool>;

public class DeleteBookCommandHandler(BookContext db) : IRequestHandler<DeleteBookCommand, bool>
{
    public async Task<bool> Handle(DeleteBookCommand req, CancellationToken ct)
    {
        var book = await db.Books.FindAsync([req.Id], ct); // await allows the thread to do this work while database is accessed improving performance
        if (book is null) return false;

        db.Books.Remove(book);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
