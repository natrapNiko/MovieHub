using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MovieHub.Data.Constants;
using MovieHub.Models;
using MovieHub.Services;
using MovieHub.ViewModels;

namespace MovieHub.Controllers;

public class MoviesController : Controller
{
    private readonly IMovieService _movieService;
    private readonly IGenreService _genreService;
    private readonly IActorService _actorService;
    private readonly IReviewService _reviewService;
    private readonly IFavoriteService _favoriteService;
    private readonly UserManager<ApplicationUser> _userManager;

    public MoviesController(
        IMovieService movieService,
        IGenreService genreService,
        IActorService actorService,
        IReviewService reviewService,
        IFavoriteService favoriteService,
        UserManager<ApplicationUser> userManager)
    {
        _movieService = movieService;
        _genreService = genreService;
        _actorService = actorService;
        _reviewService = reviewService;
        _favoriteService = favoriteService;
        _userManager = userManager;
    }

    // GET /Movies?search=batman&genreId=1&year=2008&minRating=7&director=Nolan&sort=Newest&page=1&pageSize=12
    public async Task<IActionResult> Index(
        string? search,
        int? genreId,
        int? year,
        double? minRating,
        string? director,
        MovieSortOrder sort = MovieSortOrder.Newest,
        int page = 1,
        int pageSize = 12)
    {
        var viewModel = await _movieService.SearchAsync(search, genreId, year, minRating, director, sort, page, pageSize);
        return View(viewModel);
    }

    // GET /Movies/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var movie = await _movieService.GetForDetailsAsync(id);
        if (movie is null)
        {
            return NotFound();
        }

        var currentUserId = _userManager.GetUserId(User);

        var viewModel = new MovieDetailsViewModel
        {
            Movie = movie,
            ReviewCount = movie.Reviews.Count,
            AverageRating = movie.Reviews.Count > 0 ? Math.Round(movie.Reviews.Average(r => r.Rating), 1) : 0,
            Reviews = movie.Reviews.OrderByDescending(r => r.CreatedAt).ToList(),
            YouTubeEmbedId = ExtractYouTubeId(movie.TrailerUrl),
            CurrentUserId = currentUserId
        };

        if (currentUserId is not null)
        {
            viewModel.IsFavorite = await _favoriteService.IsFavoriteAsync(id, currentUserId);
            viewModel.CurrentUserReview = await _reviewService.GetUserReviewForMovieAsync(id, currentUserId);
            viewModel.CanReview = viewModel.CurrentUserReview is null;
        }

        return View(viewModel);
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create()
    {
        var viewModel = new MovieFormViewModel
        {
            ReleaseDate = DateTime.Today,
            GenreOptions = await BuildGenreOptionsAsync(),
            ActorOptions = await BuildActorOptionsAsync()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(MovieFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            viewModel.GenreOptions = await BuildGenreOptionsAsync(viewModel.GenreId);
            viewModel.ActorOptions = await BuildActorOptionsAsync(viewModel.SelectedActorIds);
            return View(viewModel);
        }

        var movie = new Movie
        {
            Title = viewModel.Title,
            Description = viewModel.Description,
            ReleaseDate = viewModel.ReleaseDate,
            Duration = viewModel.Duration,
            Director = viewModel.Director,
            PosterUrl = viewModel.PosterUrl,
            TrailerUrl = viewModel.TrailerUrl,
            Rating = viewModel.Rating,
            GenreId = viewModel.GenreId
        };

        await _movieService.CreateAsync(movie, viewModel.SelectedActorIds);

        TempData["Success"] = $"\"{movie.Title}\" was added to the catalog.";
        return RedirectToAction(nameof(Details), new { id = movie.Id });
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Edit(int id)
    {
        var movie = await _movieService.GetForEditAsync(id);
        if (movie is null)
        {
            return NotFound();
        }

        var viewModel = new MovieFormViewModel
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            ReleaseDate = movie.ReleaseDate,
            Duration = movie.Duration,
            Director = movie.Director,
            PosterUrl = movie.PosterUrl,
            TrailerUrl = movie.TrailerUrl,
            Rating = movie.Rating,
            GenreId = movie.GenreId,
            SelectedActorIds = movie.MovieActors.Select(ma => ma.ActorId).ToList(),
            GenreOptions = await BuildGenreOptionsAsync(movie.GenreId),
            ActorOptions = await BuildActorOptionsAsync(movie.MovieActors.Select(ma => ma.ActorId))
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Edit(int id, MovieFormViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            viewModel.GenreOptions = await BuildGenreOptionsAsync(viewModel.GenreId);
            viewModel.ActorOptions = await BuildActorOptionsAsync(viewModel.SelectedActorIds);
            return View(viewModel);
        }

        var movie = new Movie
        {
            Id = viewModel.Id,
            Title = viewModel.Title,
            Description = viewModel.Description,
            ReleaseDate = viewModel.ReleaseDate,
            Duration = viewModel.Duration,
            Director = viewModel.Director,
            PosterUrl = viewModel.PosterUrl,
            TrailerUrl = viewModel.TrailerUrl,
            Rating = viewModel.Rating,
            GenreId = viewModel.GenreId
        };

        var updated = await _movieService.UpdateAsync(movie, viewModel.SelectedActorIds);
        if (!updated)
        {
            return NotFound();
        }

        TempData["Success"] = $"\"{movie.Title}\" was updated.";
        return RedirectToAction(nameof(Details), new { id = movie.Id });
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var movie = await _movieService.GetByIdAsync(id);
        if (movie is null)
        {
            return NotFound();
        }

        return View(movie);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _movieService.DeleteAsync(id);
        TempData["Success"] = "The movie was deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> BuildGenreOptionsAsync(int? selectedId = null)
    {
        var genres = await _genreService.GetAllAsync();
        return genres
            .Select(g => new SelectListItem(g.Name, g.Id.ToString(), g.Id == selectedId))
            .ToList();
    }

    private async Task<List<SelectListItem>> BuildActorOptionsAsync(IEnumerable<int>? selectedIds = null)
    {
        var selected = (selectedIds ?? Enumerable.Empty<int>()).ToHashSet();
        var (actors, _) = await _actorService.GetPagedAsync(search: null, page: 1, pageSize: 1000);
        return actors
            .Select(a => new SelectListItem(a.FullName, a.Id.ToString(), selected.Contains(a.Id)))
            .ToList();
    }

    private static string? ExtractYouTubeId(string? trailerUrl)
    {
        if (string.IsNullOrWhiteSpace(trailerUrl))
        {
            return null;
        }

        if (!Uri.TryCreate(trailerUrl, UriKind.Absolute, out var uri))
        {
            return null;
        }

        if (uri.Host.Contains("youtu.be", StringComparison.OrdinalIgnoreCase))
        {
            return uri.AbsolutePath.Trim('/');
        }

        if (uri.Host.Contains("youtube.com", StringComparison.OrdinalIgnoreCase))
        {
            if (uri.AbsolutePath.StartsWith("/embed/", StringComparison.OrdinalIgnoreCase))
            {
                return uri.AbsolutePath.Replace("/embed/", string.Empty).Trim('/');
            }

            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
            if (query.TryGetValue("v", out var videoId))
            {
                return videoId.ToString();
            }
        }

        return null;
    }
}
