using MovieHub.Models;

namespace MovieHub.Services;

public interface IReviewService
{
    Task<List<Review>> GetForMovieAsync(int movieId);

    Task<Review?> GetUserReviewForMovieAsync(int movieId, string userId);

    Task<Review?> GetByIdAsync(int id);

    Task<(bool Success, string? Error)> CreateAsync(Review review);

    Task<(bool Success, string? Error)> UpdateAsync(Review review, string currentUserId);

    Task<(bool Success, string? Error)> DeleteAsync(int reviewId, string currentUserId, bool isAdmin);

    Task<double> GetAverageRatingAsync(int movieId);

    Task<List<Review>> GetRecentAsync(int count);

    Task<List<Review>> GetAllForAdminAsync();
}
