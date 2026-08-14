using Microsoft.EntityFrameworkCore;
using MovieHub.Data;
using MovieHub.Models;

namespace MovieHub.Services;

public class FavoriteService : IFavoriteService
{
    private readonly ApplicationDbContext _context;

    public FavoriteService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Favorite>> GetForUserAsync(string userId) =>
        await _context.Favorites
            .AsNoTracking()
            .Include(f => f.Movie)
                .ThenInclude(m => m!.Genre)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

    public async Task<bool> IsFavoriteAsync(int movieId, string userId) =>
        await _context.Favorites.AsNoTracking().AnyAsync(f => f.MovieId == movieId && f.UserId == userId);

    public async Task<(bool Success, string? Error)> AddAsync(int movieId, string userId)
    {
        var alreadyExists = await _context.Favorites.AnyAsync(f => f.MovieId == movieId && f.UserId == userId);
        if (alreadyExists)
        {
            return (false, "This movie is already in your favorites.");
        }

        _context.Favorites.Add(new Favorite
        {
            MovieId = movieId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> RemoveAsync(int movieId, string userId)
    {
        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.MovieId == movieId && f.UserId == userId);

        if (favorite is null)
        {
            return false;
        }

        _context.Favorites.Remove(favorite);
        await _context.SaveChangesAsync();
        return true;
    }
}
