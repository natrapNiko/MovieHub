namespace MovieHub.ViewModels;

public class DashboardViewModel
{
    public int TotalMovies { get; set; }

    public int TotalUsers { get; set; }

    public int TotalGenres { get; set; }

    public int TotalActors { get; set; }

    public int TotalReviews { get; set; }

    public double AverageMovieRating { get; set; }

    public List<RecentReviewViewModel> RecentReviews { get; set; } = new();
}

public class RecentReviewViewModel
{
    public int Id { get; set; }

    public string MovieTitle { get; set; } = string.Empty;

    public string UserDisplayName { get; set; } = string.Empty;

    public int Rating { get; set; }

    public DateTime CreatedAt { get; set; }
}
