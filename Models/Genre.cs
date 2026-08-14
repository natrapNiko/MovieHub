using System.ComponentModel.DataAnnotations;

namespace MovieHub.Models;

public class Genre
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Genre name is required.")]
    [StringLength(50, ErrorMessage = "Genre name cannot exceed 50 characters.")]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }

    public ICollection<Movie> Movies { get; set; } = new List<Movie>();
}
