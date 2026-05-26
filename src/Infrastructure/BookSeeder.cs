using BookLibrary.Domain;
using Microsoft.EntityFrameworkCore;

namespace BookLibrary.Infrastructure;

public static class BookSeeder
{
    private static readonly string[] Titles =
    [
        "Clean Code", "The Pragmatic Programmer", "Design Patterns", "Refactoring",
        "Domain-Driven Design", "The Clean Coder", "Working Effectively with Legacy Code",
        "Code Complete", "The Mythical Man-Month", "Continuous Delivery",
        "Release It!", "Building Microservices", "Designing Data-Intensive Applications",
        "The Art of Unit Testing", "Test-Driven Development", "Agile Estimating and Planning",
        "Extreme Programming Explained", "The Software Craftsman", "Peopleware",
        "The Phoenix Project", "Accelerate", "Site Reliability Engineering",
        "Database Internals", "Concurrency in Practice", "Effective Java",
        "Programming Pearls", "Structure and Interpretation of Computer Programs",
        "Introduction to Algorithms", "The Algorithm Design Manual", "Cracking the Coding Interview"
    ];

    private static readonly string[] Authors =
    [
        "Robert C. Martin", "Andrew Hunt", "Gang of Four", "Martin Fowler",
        "Eric Evans", "Kent Beck", "Michael Feathers", "Steve McConnell",
        "Frederick Brooks", "Jez Humble", "Michael Nygard", "Sam Newman",
        "Martin Kleppmann", "Roy Osherove", "David Thomas", "Mike Cohn",
        "Ward Cunningham", "Sandro Mancuso", "Tom DeMarco", "Gene Kim",
        "Nicole Forsgren", "Betsy Beyer", "Alex Petrov", "Brian Goetz",
        "Joshua Bloch", "Jon Bentley", "Harold Abelson", "Thomas Cormen",
        "Steven Skiena", "Gayle McDowell"
    ];

    public static async Task SeedAsync(BookContext db, int count = 50_000)
    {
        if (await db.Books.AnyAsync())
        {
            Console.WriteLine("Database already seeded — skipping.");
            return;
        }

        Console.WriteLine($"Seeding {count:N0} books...");

        var random = new Random(42); // fixed seed = reproducible data
        var books = new List<Book>(count);

        for (int i = 0; i < count; i++)
        {
            var title  = Titles[random.Next(Titles.Length)];
            var author = Authors[random.Next(Authors.Length)];

            books.Add(new Book
            {
                Id     = Guid.NewGuid(),
                Title  = $"{title} Vol.{i + 1}",
                Author = author
            });
        }

        // Insert in batches of 1000 to avoid memory pressure
        const int batchSize = 1_000;
        for (int i = 0; i < books.Count; i += batchSize)
        {
            db.Books.AddRange(books.Skip(i).Take(batchSize));
            await db.SaveChangesAsync();

            if ((i / batchSize) % 10 == 0)
                Console.WriteLine($"  Inserted {Math.Min(i + batchSize, count):N0} / {count:N0}");
        }

        Console.WriteLine($"Done — {count:N0} books seeded.");
    }
}
