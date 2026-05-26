using BookLibrary.Domain;
using Dapper;
using Microsoft.Data.Sqlite;

namespace BookLibrary.Infrastructure;

public class DapperBookRepository(string connectionString) : IBookRepository
{
    private SqliteConnection Connect() => new(connectionString);

    public async Task<string> AddAsync(string title, string author, CancellationToken ct)
    {
        var id = Guid.NewGuid().ToString();
        using var conn = Connect();
        await conn.ExecuteAsync(
            "INSERT INTO Books (Id, Title, Author) VALUES (@Id, @Title, @Author)",
            new { Id = id, Title = title, Author = author });
        return id;
    }

    public async Task<List<BookDto>> GetAllAsync(CancellationToken ct)
    {
        using var conn = Connect();
        var results = await conn.QueryAsync<BookDto>(
            "SELECT Id, Title, Author FROM Books");
        return results.ToList();
    }

    public async Task<BookDto?> GetByIdAsync(string id, CancellationToken ct)
    {
        using var conn = Connect();
        return await conn.QuerySingleOrDefaultAsync<BookDto>(
            "SELECT Id, Title, Author FROM Books WHERE Id = @Id",
            new { Id = id });
    }

    public async Task<List<BookDto>> SearchAsync(string term, CancellationToken ct)
    {
        using var conn = Connect();
        var results = await conn.QueryAsync<BookDto>(
            "SELECT Id, Title, Author FROM Books WHERE LOWER(Title) LIKE @term OR LOWER(Author) LIKE @term",
            new { term = $"%{term.ToLower()}%" });
        return results.ToList();
    }

    public async Task<bool> UpdateAsync(string id, string title, string author, CancellationToken ct)
    {
        using var conn = Connect();
        var rows = await conn.ExecuteAsync(
            "UPDATE Books SET Title = @Title, Author = @Author WHERE Id = @Id",
            new { Id = id, Title = title, Author = author });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct)
    {
        using var conn = Connect();
        var rows = await conn.ExecuteAsync(
            "DELETE FROM Books WHERE Id = @Id",
            new { Id = id });
        return rows > 0;
    }
}
