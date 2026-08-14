using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MovieHub.Models;
using MovieHub.Services;

namespace MovieHub.Controllers;

[Authorize]
public class FavoritesController : Controller
{
    private readonly IFavoriteService _favoriteService;
    private readonly UserManager<ApplicationUser> _userManager;

    public FavoritesController(IFavoriteService favoriteService, UserManager<ApplicationUser> userManager)
    {
        _favoriteService = favoriteService;
        _userManager = userManager;
    }

    // GET /Favorites
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var favorites = await _favoriteService.GetForUserAsync(userId);
        return View(favorites);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int movieId, string? returnUrl = null)
    {
        var userId = _userManager.GetUserId(User)!;
        var (success, error) = await _favoriteService.AddAsync(movieId, userId);

        TempData[success ? "Success" : "Info"] = success ? "Added to your favorites." : error;

        return RedirectToLocalOrDetails(returnUrl, movieId);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int movieId, string? returnUrl = null)
    {
        var userId = _userManager.GetUserId(User)!;
        await _favoriteService.RemoveAsync(movieId, userId);

        TempData["Success"] = "Removed from your favorites.";

        return RedirectToLocalOrDetails(returnUrl, movieId);
    }

    private IActionResult RedirectToLocalOrDetails(string? returnUrl, int movieId)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Details", "Movies", new { id = movieId });
    }
}
