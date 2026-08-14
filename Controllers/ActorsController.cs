using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieHub.Data.Constants;
using MovieHub.Models;
using MovieHub.Services;
using MovieHub.ViewModels;

namespace MovieHub.Controllers;

public class ActorsController : Controller
{
    private readonly IActorService _actorService;

    public ActorsController(IActorService actorService)
    {
        _actorService = actorService;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        const int pageSize = 20;
        var (actors, totalCount) = await _actorService.GetPagedAsync(search, page, pageSize);

        ViewBag.Search = search;
        ViewBag.Pagination = new PaginationViewModel
        {
            CurrentPage = page,
            PageSize = pageSize,
            TotalItems = totalCount,
            Action = "Index"
        };

        return View(actors);
    }

    public async Task<IActionResult> Details(int id)
    {
        var actor = await _actorService.GetWithMoviesAsync(id);
        if (actor is null)
        {
            return NotFound();
        }

        return View(actor);
    }

    [Authorize(Roles = Roles.Admin)]
    public IActionResult Create() => View(new ActorFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(ActorFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var actor = new Actor
        {
            FirstName = viewModel.FirstName,
            LastName = viewModel.LastName,
            Biography = viewModel.Biography,
            BirthDate = viewModel.BirthDate,
            PhotoUrl = viewModel.PhotoUrl
        };

        await _actorService.CreateAsync(actor);
        TempData["Success"] = $"{actor.FullName} was added.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Edit(int id)
    {
        var actor = await _actorService.GetByIdAsync(id);
        if (actor is null)
        {
            return NotFound();
        }

        var viewModel = new ActorFormViewModel
        {
            Id = actor.Id,
            FirstName = actor.FirstName,
            LastName = actor.LastName,
            Biography = actor.Biography,
            BirthDate = actor.BirthDate,
            PhotoUrl = actor.PhotoUrl
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Edit(int id, ActorFormViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var actor = new Actor
        {
            Id = viewModel.Id,
            FirstName = viewModel.FirstName,
            LastName = viewModel.LastName,
            Biography = viewModel.Biography,
            BirthDate = viewModel.BirthDate,
            PhotoUrl = viewModel.PhotoUrl
        };

        var updated = await _actorService.UpdateAsync(actor);
        if (!updated)
        {
            return NotFound();
        }

        TempData["Success"] = $"{actor.FirstName} {actor.LastName} was updated.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var actor = await _actorService.GetByIdAsync(id);
        if (actor is null)
        {
            return NotFound();
        }

        return View(actor);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _actorService.DeleteAsync(id);
        TempData["Success"] = "The actor was deleted.";
        return RedirectToAction(nameof(Index));
    }
}
