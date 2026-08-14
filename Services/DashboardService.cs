using Microsoft.EntityFrameworkCore;
using MovieHub.Data;
using MovieHub.ViewModels;

namespace MovieHub.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        var totalReviews = await _context.Reviews.CountAsync();
        var averageRating = totalReviews == 0
            ? 0
            : Math.Round(await _context.Reviews.AverageAsync(r => r.Rating), 1);

        var recentReviews = await _context.Reviews
            .AsNoTracking()
            .Include(r => r.Movie)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Take(10)
            .Select(r => new RecentReviewViewModel
            {
                Id = r.Id,
                MovieTitle = r.Movie != null ? r.Movie.Title : string.Empty,
                UserDisplayName = r.User != null ? (r.User.DisplayName != "" ? r.User.DisplayName : r.User.Email!) : "Unknown",
                Rating = r.Rating,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new DashboardViewModel
        {
            TotalMovies = await _context.Movies.CountAsync(),
            TotalUsers = await _context.Users.CountAsync(),
            TotalGenres = await _context.Genres.CountAsync(),
            TotalActors = await _context.Actors.CountAsync(),
            TotalReviews = totalReviews,
            AverageMovieRating = averageRating,
            RecentReviews = recentReviews
        };
    }
}
