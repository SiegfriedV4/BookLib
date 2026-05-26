namespace BookLibrary.Domain;

public class BookDto
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Author { get; init; } = "";
}

public interface IBookRepository
{
    Task<string> AddAsync(string title, string author, CancellationToken ct);
    Task<List<BookDto>> GetAllAsync(CancellationToken ct);
    Task<BookDto?> GetByIdAsync(string id, CancellationToken ct);
    Task<List<BookDto>> SearchAsync(string term, CancellationToken ct);
    Task<bool> UpdateAsync(string id, string title, string author, CancellationToken ct);
    Task<bool> DeleteAsync(string id, CancellationToken ct);
}
