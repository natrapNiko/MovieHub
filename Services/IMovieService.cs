using MovieHub.Models;
using MovieHub.ViewModels;

namespace MovieHub.Services;

public interface IMovieService
{
    Task<MovieListViewModel> SearchAsync(
        string? search,
        int? genreId,
        int? year,
        double? minRating,
        string? director,
        MovieSortOrder sort,
        int page,
        int pageSize);

    Task<Movie?> GetByIdAsync(int id);

    Task<Movie?> GetForDetailsAsync(int id);

    Task<Movie?> GetForEditAsync(int id);

    Task<Movie> CreateAsync(Movie movie, IEnumerable<int> actorIds);

    Task<bool> UpdateAsync(Movie movie, IEnumerable<int> actorIds);

    Task<bool> DeleteAsync(int id);

    Task<bool> ExistsAsync(int id);

    Task<HomeViewModel> GetHomePageDataAsync();

    Task<List<int>> GetAvailableYearsAsync();
}
