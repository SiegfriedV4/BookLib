using Dapper;
using Microsoft.Data.Sqlite;

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

    public static async Task SeedAsync(string connectionString, int count = 20_000)
    {
        using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync();

        var existing = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Books");
        if (existing > 0)
        {
            Console.WriteLine("Database already seeded — skipping.");
            return;
        }

        Console.WriteLine($"Seeding {count:N0} books...");

        var random = new Random(42);
        const int batchSize = 1_000;

        for (int i = 0; i < count; i += batchSize)
        {
            var batch = Enumerable.Range(i, Math.Min(batchSize, count - i)).Select(n =>
            {
                var title  = Titles[random.Next(Titles.Length)];
                var author = Authors[random.Next(Authors.Length)];
                return new { Id = Guid.NewGuid().ToString(), Title = $"{title} Vol.{n + 1}", Author = author };
            }).ToList();

            using var tx = conn.BeginTransaction();
            await conn.ExecuteAsync(
                "INSERT INTO Books (Id, Title, Author) VALUES (@Id, @Title, @Author)",
                batch, tx);
            await tx.CommitAsync();

            if ((i / batchSize) % 10 == 0)
                Console.WriteLine($"  Inserted {Math.Min(i + batchSize, count):N0} / {count:N0}");
        }

        Console.WriteLine($"Done — {count:N0} books seeded.");
    }
}
