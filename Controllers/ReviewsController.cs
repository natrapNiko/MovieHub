using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MovieHub.Data.Constants;
using MovieHub.Models;
using MovieHub.Services;
using MovieHub.ViewModels;

namespace MovieHub.Controllers;

[Authorize]
public class ReviewsController : Controller
{
    private readonly IReviewService _reviewService;
    private readonly IMovieService _movieService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReviewsController(IReviewService reviewService, IMovieService movieService, UserManager<ApplicationUser> userManager)
    {
        _reviewService = reviewService;
        _movieService = movieService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Create(int movieId)
    {
        var movie = await _movieService.GetByIdAsync(movieId);
        if (movie is null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User)!;
        var existingReview = await _reviewService.GetUserReviewForMovieAsync(movieId, userId);
        if (existingReview is not null)
        {
            TempData["Info"] = "You have already reviewed this movie. You can edit your existing review below.";
            return RedirectToAction(nameof(Edit), new { id = existingReview.Id });
        }

        var viewModel = new ReviewFormViewModel
        {
            MovieId = movie.Id,
            MovieTitle = movie.Title
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReviewFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var review = new Review
        {
            MovieId = viewModel.MovieId,
            UserId = _userManager.GetUserId(User)!,
            Rating = viewModel.Rating,
            Comment = viewModel.Comment
        };

        var (success, error) = await _reviewService.CreateAsync(review);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            return View(viewModel);
        }

        TempData["Success"] = "Your review was posted.";
        return RedirectToAction("Details", "Movies", new { id = viewModel.MovieId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var review = await _reviewService.GetByIdAsync(id);
        if (review is null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        if (review.UserId != userId)
        {
            return Forbid();
        }

        var viewModel = new ReviewFormViewModel
        {
            Id = review.Id,
            MovieId = review.MovieId,
            MovieTitle = review.Movie?.Title ?? string.Empty,
            Rating = review.Rating,
            Comment = review.Comment
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ReviewFormViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var review = new Review
        {
            Id = viewModel.Id,
            Rating = viewModel.Rating,
            Comment = viewModel.Comment
        };

        var userId = _userManager.GetUserId(User)!;
        var (success, error) = await _reviewService.UpdateAsync(review, userId);
        if (!success)
        {
            if (error == "You can only edit your own reviews.")
            {
                return Forbid();
            }

            return NotFound();
        }

        TempData["Success"] = "Your review was updated.";
        return RedirectToAction("Details", "Movies", new { id = viewModel.MovieId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int movieId)
    {
        var userId = _userManager.GetUserId(User)!;
        var isAdmin = User.IsInRole(Roles.Admin);

        var (success, error) = await _reviewService.DeleteAsync(id, userId, isAdmin);
        if (!success && error == "You can only delete your own reviews.")
        {
            return Forbid();
        }

        TempData["Success"] = "The review was deleted.";
        return RedirectToAction("Details", "Movies", new { id = movieId });
    }
}
