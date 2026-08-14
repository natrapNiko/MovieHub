using MovieHub.Models;

namespace MovieHub.Services;

public interface IFavoriteService
{
    Task<List<Favorite>> GetForUserAsync(string userId);

    Task<bool> IsFavoriteAsync(int movieId, string userId);

    Task<(bool Success, string? Error)> AddAsync(int movieId, string userId);

    Task<bool> RemoveAsync(int movieId, string userId);
}
