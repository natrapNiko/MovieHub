using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MovieHub.ViewModels;

//Used by the admin Create/Edit movie views. Keeps the entity free of
//UI-only concerns such as the multi-select actor list.
public class MovieFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(4000)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Release date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Release Date")]
    public DateTime ReleaseDate { get; set; } = DateTime.Today;

    [Required]
    [Range(1, 1000, ErrorMessage = "Duration must be greater than 0 minutes.")]
    public int Duration { get; set; }

    [Required(ErrorMessage = "Director is required.")]
    [StringLength(150)]
    public string Director { get; set; } = string.Empty;

    [StringLength(500)]
    [Url(ErrorMessage = "Please provide a valid URL.")]
    [Display(Name = "Poster URL")]
    public string? PosterUrl { get; set; }

    [StringLength(500)]
    [Url(ErrorMessage = "Please provide a valid URL.")]
    [Display(Name = "Trailer URL")]
    public string? TrailerUrl { get; set; }

    [Range(0, 10, ErrorMessage = "Rating must be between 0 and 10.")]
    public double Rating { get; set; }

    [Required(ErrorMessage = "Please select a genre.")]
    [Display(Name = "Genre")]
    public int GenreId { get; set; }

    [Display(Name = "Cast")]
    public List<int> SelectedActorIds { get; set; } = new();

    public List<SelectListItem> GenreOptions { get; set; } = new();

    public List<SelectListItem> ActorOptions { get; set; } = new();
}
