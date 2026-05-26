namespace BookLibrary.Domain;

public class BookDto
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Author { get; init; } = "";
}

public class AuthorStatsDto
{
    public string Author { get; init; } = "";
    public int BookCount { get; init; }
}

public class PagedBooksDto
{
    public List<BookDto> Books { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}

public interface IBookRepository
{
    Task<string> AddAsync(string title, string author, CancellationToken ct);
    Task<List<BookDto>> GetAllAsync(CancellationToken ct);
    Task<BookDto?> GetByIdAsync(string id, CancellationToken ct);
    Task<List<BookDto>> SearchAsync(string term, CancellationToken ct);
    Task<bool> UpdateAsync(string id, string title, string author, CancellationToken ct);
    Task<bool> DeleteAsync(string id, CancellationToken ct);

    // Heavy queries
    Task<PagedBooksDto> GetPagedAsync(int page, int pageSize, CancellationToken ct);
    Task<List<AuthorStatsDto>> GetAuthorStatsAsync(CancellationToken ct);
    Task<List<BookDto>> GetBooksByAuthorAsync(string author, CancellationToken ct);
    Task<int> CountAsync(CancellationToken ct);
}
