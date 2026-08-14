using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MovieHub.Data;
using MovieHub.Models;
using MovieHub.ViewModels;

namespace MovieHub.Services;

public class MovieService : IMovieService
{
    private readonly ApplicationDbContext _context;

    // Defined as an Expression<Func<>> (not a compiled delegate) so EF Core can
    // translate the projection into SQL wherever it is reused below, instead of
    // pulling entire entities into memory and mapping client-side.
    private static readonly Expression<Func<Movie, MovieCardViewModel>> ToCardProjection = m => new MovieCardViewModel
    {
        Id = m.Id,
        Title = m.Title,
        ReleaseYear = m.ReleaseDate.Year,
        GenreName = m.Genre != null ? m.Genre.Name : string.Empty,
        AverageRating = m.Reviews.Any() ? Math.Round(m.Reviews.Average(r => r.Rating), 1) : m.Rating,
        ReviewCount = m.Reviews.Count,
        PosterUrl = m.PosterUrl,
        ShortDescription = m.Description.Length > 140 ? m.Description.Substring(0, 140) + "..." : m.Description
    };

    public MovieService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MovieListViewModel> SearchAsync(
        string? search,
        int? genreId,
        int? year,
        double? minRating,
        string? director,
        MovieSortOrder sort,
        int page,
        int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 12 : pageSize;

        var query = _context.Movies.AsNoTracking().Include(m => m.Genre).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            // Search across title, director, genre name and cast — all pushed down to SQL.
            query = query.Where(m =>
                m.Title.Contains(search) ||
                m.Director.Contains(search) ||
                (m.Genre != null && m.Genre.Name.Contains(search)) ||
                m.MovieActors.Any(ma =>
                    (ma.Actor!.FirstName + " " + ma.Actor.LastName).Contains(search)));
        }

        if (genreId.HasValue)
        {
            query = query.Where(m => m.GenreId == genreId.Value);
        }

        if (year.HasValue)
        {
            query = query.Where(m => m.ReleaseDate.Year == year.Value);
        }

        if (minRating.HasValue)
        {
            query = query.Where(m => m.Rating >= minRating.Value);
        }

        if (!string.IsNullOrWhiteSpace(director))
        {
            query = query.Where(m => m.Director.Contains(director));
        }

        query = sort switch
        {
            MovieSortOrder.TitleAsc => query.OrderBy(m => m.Title),
            MovieSortOrder.TitleDesc => query.OrderByDescending(m => m.Title),
            MovieSortOrder.Oldest => query.OrderBy(m => m.ReleaseDate),
            MovieSortOrder.HighestRated => query.OrderByDescending(m => m.Rating),
            MovieSortOrder.LowestRated => query.OrderBy(m => m.Rating),
            _ => query.OrderByDescending(m => m.ReleaseDate)
        };

        var totalCount = await query.CountAsync();

        var movies = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToCardProjection)
            .ToListAsync();

        var genres = await _context.Genres
            .AsNoTracking()
            .OrderBy(g => g.Name)
            .Select(g => new GenreSummaryViewModel
            {
                Id = g.Id,
                Name = g.Name,
                MovieCount = g.Movies.Count
            })
            .ToListAsync();

        return new MovieListViewModel
        {
            Movies = movies,
            Pagination = new PaginationViewModel
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalCount
            },
            Search = search,
            GenreId = genreId,
            Year = year,
            MinRating = minRating,
            Director = director,
            Sort = sort,
            AvailableGenres = genres,
            AvailableYears = await GetAvailableYearsAsync()
        };
    }

    public async Task<Movie?> GetByIdAsync(int id) =>
        await _context.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);

    public async Task<Movie?> GetForDetailsAsync(int id) =>
        await _context.Movies
            .AsNoTracking()
            .Include(m => m.Genre)
            .Include(m => m.MovieActors)
                .ThenInclude(ma => ma.Actor)
            .Include(m => m.Reviews)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<Movie?> GetForEditAsync(int id) =>
        await _context.Movies
            .Include(m => m.MovieActors)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<Movie> CreateAsync(Movie movie, IEnumerable<int> actorIds)
    {
        movie.CreatedAt = DateTime.UtcNow;
        movie.UpdatedAt = DateTime.UtcNow;

        foreach (var actorId in actorIds.Distinct())
        {
            movie.MovieActors.Add(new MovieActor { ActorId = actorId });
        }

        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();
        return movie;
    }

    public async Task<bool> UpdateAsync(Movie movie, IEnumerable<int> actorIds)
    {
        var existing = await _context.Movies
            .Include(m => m.MovieActors)
            .FirstOrDefaultAsync(m => m.Id == movie.Id);

        if (existing is null)
        {
            return false;
        }

        existing.Title = movie.Title;
        existing.Description = movie.Description;
        existing.ReleaseDate = movie.ReleaseDate;
        existing.Duration = movie.Duration;
        existing.Director = movie.Director;
        existing.PosterUrl = movie.PosterUrl;
        existing.TrailerUrl = movie.TrailerUrl;
        existing.Rating = movie.Rating;
        existing.GenreId = movie.GenreId;
        existing.UpdatedAt = DateTime.UtcNow;

        var newActorIds = actorIds.Distinct().ToHashSet();
        var currentActorIds = existing.MovieActors.Select(ma => ma.ActorId).ToHashSet();

        // Remove actors no longer selected.
        existing.MovieActors
            .Where(ma => !newActorIds.Contains(ma.ActorId))
            .ToList()
            .ForEach(ma => existing.MovieActors.Remove(ma));

        // Add newly selected actors.
        foreach (var actorId in newActorIds.Except(currentActorIds))
        {
            existing.MovieActors.Add(new MovieActor { MovieId = existing.Id, ActorId = actorId });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var movie = await _context.Movies.FindAsync(id);
        if (movie is null)
        {
            return false;
        }

        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id) =>
        await _context.Movies.AsNoTracking().AnyAsync(m => m.Id == id);

    public async Task<HomeViewModel> GetHomePageDataAsync()
    {
        const int railSize = 8;

        var featured = await _context.Movies
            .AsNoTracking()
            .Include(m => m.Genre)
            .OrderByDescending(m => m.Rating)
            .Take(railSize)
            .Select(ToCardProjection)
            .ToListAsync();

        var popular = await _context.Movies
            .AsNoTracking()
            .Include(m => m.Genre)
            .OrderByDescending(m => m.Reviews.Count)
            .ThenByDescending(m => m.Rating)
            .Take(railSize)
            .Select(ToCardProjection)
            .ToListAsync();

        var recentlyAdded = await _context.Movies
            .AsNoTracking()
            .Include(m => m.Genre)
            .OrderByDescending(m => m.CreatedAt)
            .Take(railSize)
            .Select(ToCardProjection)
            .ToListAsync();

        var highestRated = await _context.Movies
            .AsNoTracking()
            .Include(m => m.Genre)
            .Where(m => m.Reviews.Any())
            .OrderByDescending(m => m.Reviews.Average(r => r.Rating))
            .Take(railSize)
            .Select(ToCardProjection)
            .ToListAsync();

        var genres = await _context.Genres
            .AsNoTracking()
            .OrderBy(g => g.Name)
            .Select(g => new GenreSummaryViewModel
            {
                Id = g.Id,
                Name = g.Name,
                MovieCount = g.Movies.Count
            })
            .ToListAsync();

        return new HomeViewModel
        {
            FeaturedMovies = featured,
            PopularMovies = popular,
            RecentlyAddedMovies = recentlyAdded,
            HighestRatedMovies = highestRated,
            Genres = genres
        };
    }

    public async Task<List<int>> GetAvailableYearsAsync() =>
        await _context.Movies
            .AsNoTracking()
            .Select(m => m.ReleaseDate.Year)
            .Distinct()
            .OrderByDescending(y => y)
            .ToListAsync();
}
