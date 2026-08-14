using MovieHub.Models;

namespace MovieHub.ViewModels;

public class MovieDetailsViewModel
{
    public Movie Movie { get; set; } = null!;

    public double AverageRating { get; set; }

    public int ReviewCount { get; set; }

    public List<Review> Reviews { get; set; } = new();

    public bool IsFavorite { get; set; }

    public Review? CurrentUserReview { get; set; }

    public bool CanReview { get; set; }

    public string? YouTubeEmbedId { get; set; }

    public string? CurrentUserId { get; set; }
}
