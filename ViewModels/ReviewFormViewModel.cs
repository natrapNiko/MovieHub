using System.ComponentModel.DataAnnotations;

namespace MovieHub.ViewModels;

public class ReviewFormViewModel
{
    public int Id { get; set; }

    [Required]
    public int MovieId { get; set; }

    public string MovieTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "A rating is required.")]
    [Range(1, 10, ErrorMessage = "Rating must be between 1 and 10.")]
    public int Rating { get; set; } = 5;

    [Required(ErrorMessage = "Please write a comment for your review.")]
    [StringLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters.")]
    public string Comment { get; set; } = string.Empty;
}
