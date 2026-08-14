using Microsoft.EntityFrameworkCore;
using MovieHub.Data;
using MovieHub.Models;

namespace MovieHub.Services;

public class ActorService : IActorService
{
    private readonly ApplicationDbContext _context;

    public ActorService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Actor> Actors, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize)
    {
        var query = _context.Actors.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(a =>
                a.FirstName.Contains(search) ||
                a.LastName.Contains(search));
        }

        var totalCount = await query.CountAsync();

        var actors = await query
            .OrderBy(a => a.LastName)
            .ThenBy(a => a.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (actors, totalCount);
    }

    public async Task<Actor?> GetByIdAsync(int id) =>
        await _context.Actors.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);

    public async Task<Actor?> GetWithMoviesAsync(int id) =>
        await _context.Actors
            .AsNoTracking()
            .Include(a => a.MovieActors)
                .ThenInclude(ma => ma.Movie)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<Actor> CreateAsync(Actor actor)
    {
        _context.Actors.Add(actor);
        await _context.SaveChangesAsync();
        return actor;
    }

    public async Task<bool> UpdateAsync(Actor actor)
    {
        var existing = await _context.Actors.FindAsync(actor.Id);
        if (existing is null)
        {
            return false;
        }

        existing.FirstName = actor.FirstName;
        existing.LastName = actor.LastName;
        existing.Biography = actor.Biography;
        existing.BirthDate = actor.BirthDate;
        existing.PhotoUrl = actor.PhotoUrl;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var actor = await _context.Actors.FindAsync(id);
        if (actor is null)
        {
            return false;
        }

        _context.Actors.Remove(actor);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id) =>
        await _context.Actors.AsNoTracking().AnyAsync(a => a.Id == id);
}
