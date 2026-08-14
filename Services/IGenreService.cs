using MovieHub.Models;

namespace MovieHub.Services;

public interface IGenreService
{
    Task<List<Genre>> GetAllAsync();

    Task<Genre?> GetByIdAsync(int id);

    Task<Genre?> GetWithMoviesAsync(int id);

    Task<Genre> CreateAsync(Genre genre);

    Task<bool> UpdateAsync(Genre genre);

    Task<bool> DeleteAsync(int id);

    Task<bool> ExistsAsync(int id);
}
