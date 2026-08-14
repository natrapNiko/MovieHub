using System.ComponentModel.DataAnnotations;

namespace MovieHub.Models;

public class Movie
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(4000, ErrorMessage = "Description cannot exceed 4000 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Release date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Release Date")]
    public DateTime ReleaseDate { get; set; }

    [Required(ErrorMessage = "Duration is required.")]
    [Range(1, 1000, ErrorMessage = "Duration must be greater than 0 minutes.")]
    [Display(Name = "Duration (minutes)")]
    public int Duration { get; set; }

    [Required(ErrorMessage = "Director is required.")]
    [StringLength(150)]
    public string Director { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Poster URL")]
    [Url(ErrorMessage = "Please provide a valid URL.")]
    public string? PosterUrl { get; set; }

    [StringLength(500)]
    [Display(Name = "Trailer URL")]
    [Url(ErrorMessage = "Please provide a valid URL.")]
    public string? TrailerUrl { get; set; }

    /// <summary>
    /// Editorial base rating (0-10) set by an admin. The rating displayed to users
    /// is generally the average of user reviews; this acts as a fallback/seed value.
    /// </summary>
    [Range(0, 10, ErrorMessage = "Rating must be between 0 and 10.")]
    public double Rating { get; set; }

    [Display(Name = "Added On")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Last Updated")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "Please select a genre.")]
    [Display(Name = "Genre")]
    public int GenreId { get; set; }

    public Genre? Genre { get; set; }

    public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();

    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    [Display(Name = "Release Year")]
    public int ReleaseYear => ReleaseDate.Year;
}
