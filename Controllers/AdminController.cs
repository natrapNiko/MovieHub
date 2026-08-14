using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieHub.Data.Constants;
using MovieHub.Models;
using MovieHub.Services;
using MovieHub.ViewModels;

namespace MovieHub.Controllers;

/// <summary>
/// Admin-only dashboard plus the two management areas (users, reviews) that
/// don't already have a dedicated public controller. Movies, genres and
/// actors are managed from their own controllers (MoviesController etc.) —
/// those views show Create/Edit/Delete actions only when the signed-in user
/// is in the Admin role, so there is a single source of truth for that CRUD.
/// </summary>
[Authorize(Roles = Roles.Admin)]
[Route("Admin")]
public class AdminController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IReviewService _reviewService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(IDashboardService dashboardService, IReviewService reviewService, UserManager<ApplicationUser> userManager)
    {
        _dashboardService = dashboardService;
        _reviewService = reviewService;
        _userManager = userManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var viewModel = await _dashboardService.GetDashboardAsync();
        return View(viewModel);
    }

    [HttpGet("Users")]
    public async Task<IActionResult> Users()
    {
        var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();

        var rows = new List<AdminUserRowViewModel>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            rows.Add(new AdminUserRowViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                DisplayName = user.DisplayName,
                CreatedAt = user.CreatedAt,
                IsAdmin = roles.Contains(Roles.Admin)
            });
        }

        return View(rows);
    }

    [HttpPost("Users/ToggleAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAdminRole(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Info"] = "You cannot change your own admin status.";
            return RedirectToAction(nameof(Users));
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, Roles.Admin);
        if (isAdmin)
        {
            await _userManager.RemoveFromRoleAsync(user, Roles.Admin);
        }
        else
        {
            await _userManager.AddToRoleAsync(user, Roles.Admin);
        }

        TempData["Success"] = $"Updated admin status for {user.Email}.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost("Users/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        if (userId == _userManager.GetUserId(User))
        {
            TempData["Info"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Users));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is not null)
        {
            await _userManager.DeleteAsync(user);
            TempData["Success"] = $"Deleted account for {user.Email}.";
        }

        return RedirectToAction(nameof(Users));
    }

    [HttpGet("Reviews")]
    public async Task<IActionResult> Reviews()
    {
        var reviews = await _reviewService.GetAllForAdminAsync();
        return View(reviews);
    }

    [HttpPost("Reviews/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var currentUserId = _userManager.GetUserId(User)!;
        await _reviewService.DeleteAsync(id, currentUserId, isAdmin: true);
        TempData["Success"] = "The review was deleted.";
        return RedirectToAction(nameof(Reviews));
    }
}
