using MovieHub.Models;

namespace MovieHub.Services;

public interface IActorService
{
    Task<(List<Actor> Actors, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize);

    Task<Actor?> GetByIdAsync(int id);

    Task<Actor?> GetWithMoviesAsync(int id);

    Task<Actor> CreateAsync(Actor actor);

    Task<bool> UpdateAsync(Actor actor);

    Task<bool> DeleteAsync(int id);

    Task<bool> ExistsAsync(int id);
}
