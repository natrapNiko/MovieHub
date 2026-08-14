using Microsoft.EntityFrameworkCore;
using MovieHub.Data;
using MovieHub.Models;

namespace MovieHub.Services;

public class GenreService : IGenreService
{
    private readonly ApplicationDbContext _context;

    public GenreService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Genre>> GetAllAsync() =>
        await _context.Genres.AsNoTracking().OrderBy(g => g.Name).ToListAsync();

    public async Task<Genre?> GetByIdAsync(int id) =>
        await _context.Genres.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);

    public async Task<Genre?> GetWithMoviesAsync(int id) =>
        await _context.Genres
            .AsNoTracking()
            .Include(g => g.Movies)
            .FirstOrDefaultAsync(g => g.Id == id);

    public async Task<Genre> CreateAsync(Genre genre)
    {
        _context.Genres.Add(genre);
        await _context.SaveChangesAsync();
        return genre;
    }

    public async Task<bool> UpdateAsync(Genre genre)
    {
        var existing = await _context.Genres.FindAsync(genre.Id);
        if (existing is null)
        {
            return false;
        }

        existing.Name = genre.Name;
        existing.Description = genre.Description;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var genre = await _context.Genres.FindAsync(id);
        if (genre is null)
        {
            return false;
        }

        _context.Genres.Remove(genre);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id) =>
        await _context.Genres.AsNoTracking().AnyAsync(g => g.Id == id);
}
