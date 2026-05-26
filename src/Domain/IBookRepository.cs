namespace BookLibrary.Domain;

public record BookDto(Guid Id, string Title, string Author);

public interface IBookRepository
{
    Task<Guid> AddAsync(string title, string author, CancellationToken ct);
    Task<List<BookDto>> GetAllAsync(CancellationToken ct);
    Task<BookDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<BookDto>> SearchAsync(string term, CancellationToken ct);
    Task<bool> UpdateAsync(Guid id, string title, string author, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
