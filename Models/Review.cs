using System.ComponentModel.DataAnnotations;

namespace MovieHub.Models;

public class Review
{
    public int Id { get; set; }

    public int MovieId { get; set; }

    public Movie? Movie { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    [Required(ErrorMessage = "A rating is required.")]
    [Range(1, 10, ErrorMessage = "Rating must be between 1 and 10.")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Please write a comment for your review.")]
    [StringLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters.")]
    public string Comment { get; set; } = string.Empty;

    [Display(Name = "Posted On")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Last Updated")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
