using Microsoft.AspNetCore.Mvc;
using MovieHub.Services;

namespace MovieHub.Controllers;

public class HomeController : Controller
{
    private readonly IMovieService _movieService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IMovieService movieService, ILogger<HomeController> logger)
    {
        _movieService = movieService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = await _movieService.GetHomePageDataAsync();
        return View(viewModel);
    }
}
