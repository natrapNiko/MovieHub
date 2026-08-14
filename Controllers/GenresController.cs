using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieHub.Data.Constants;
using MovieHub.Models;
using MovieHub.Services;

namespace MovieHub.Controllers;

public class GenresController : Controller
{
    private readonly IGenreService _genreService;

    public GenresController(IGenreService genreService)
    {
        _genreService = genreService;
    }

    public async Task<IActionResult> Index()
    {
        var genres = await _genreService.GetAllAsync();
        return View(genres);
    }

    public async Task<IActionResult> Details(int id)
    {
        var genre = await _genreService.GetWithMoviesAsync(id);
        if (genre is null)
        {
            return NotFound();
        }

        return View(genre);
    }

    [Authorize(Roles = Roles.Admin)]
    public IActionResult Create() => View(new Genre());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(Genre genre)
    {
        if (!ModelState.IsValid)
        {
            return View(genre);
        }

        await _genreService.CreateAsync(genre);
        TempData["Success"] = $"Genre \"{genre.Name}\" was created.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Edit(int id)
    {
        var genre = await _genreService.GetByIdAsync(id);
        if (genre is null)
        {
            return NotFound();
        }

        return View(genre);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Edit(int id, Genre genre)
    {
        if (id != genre.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(genre);
        }

        var updated = await _genreService.UpdateAsync(genre);
        if (!updated)
        {
            return NotFound();
        }

        TempData["Success"] = $"Genre \"{genre.Name}\" was updated.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var genre = await _genreService.GetByIdAsync(id);
        if (genre is null)
        {
            return NotFound();
        }

        return View(genre);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _genreService.DeleteAsync(id);
            TempData["Success"] = "The genre was deleted.";
        }
        catch (DbUpdateException)
        {
            TempData["Info"] = "This genre still has movies assigned to it. Reassign or delete those movies first.";
        }

        return RedirectToAction(nameof(Index));
    }
}
