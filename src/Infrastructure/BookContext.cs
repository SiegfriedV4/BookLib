using BookLibrary.Domain;
using Microsoft.EntityFrameworkCore;

namespace BookLibrary.Infrastructure;

public class BookContext(DbContextOptions<BookContext> options) : DbContext(options)
{
    public DbSet<Book> Books { get; set; } = null!;
}
