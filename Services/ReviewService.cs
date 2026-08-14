using Microsoft.EntityFrameworkCore;
using MovieHub.Data;
using MovieHub.Models;

namespace MovieHub.Services;

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _context;

    public ReviewService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Review>> GetForMovieAsync(int movieId) =>
        await _context.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Where(r => r.MovieId == movieId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<Review?> GetUserReviewForMovieAsync(int movieId, string userId) =>
        await _context.Reviews
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.MovieId == movieId && r.UserId == userId);

    public async Task<Review?> GetByIdAsync(int id) =>
        await _context.Reviews.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<(bool Success, string? Error)> CreateAsync(Review review)
    {
        var alreadyReviewed = await _context.Reviews
            .AnyAsync(r => r.MovieId == review.MovieId && r.UserId == review.UserId);

        if (alreadyReviewed)
        {
            return (false, "You have already reviewed this movie. You can edit your existing review instead.");
        }

        review.CreatedAt = DateTime.UtcNow;
        review.UpdatedAt = DateTime.UtcNow;

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(Review review, string currentUserId)
    {
        var existing = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == review.Id);
        if (existing is null)
        {
            return (false, "Review not found.");
        }

        if (existing.UserId != currentUserId)
        {
            return (false, "You can only edit your own reviews.");
        }

        existing.Rating = review.Rating;
        existing.Comment = review.Comment;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int reviewId, string currentUserId, bool isAdmin)
    {
        var existing = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId);
        if (existing is null)
        {
            return (false, "Review not found.");
        }

        if (existing.UserId != currentUserId && !isAdmin)
        {
            return (false, "You can only delete your own reviews.");
        }

        _context.Reviews.Remove(existing);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<double> GetAverageRatingAsync(int movieId)
    {
        var hasReviews = await _context.Reviews.AnyAsync(r => r.MovieId == movieId);
        if (!hasReviews)
        {
            return 0;
        }

        return Math.Round(await _context.Reviews
            .Where(r => r.MovieId == movieId)
            .AverageAsync(r => r.Rating), 1);
    }

    public async Task<List<Review>> GetRecentAsync(int count) =>
        await _context.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Movie)
            .OrderByDescending(r => r.CreatedAt)
            .Take(count)
            .ToListAsync();

    public async Task<List<Review>> GetAllForAdminAsync() =>
        await _context.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Movie)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
}
