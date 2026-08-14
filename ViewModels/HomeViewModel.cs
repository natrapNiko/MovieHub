namespace MovieHub.ViewModels;

public class HomeViewModel
{
    public List<MovieCardViewModel> FeaturedMovies { get; set; } = new();

    public List<MovieCardViewModel> PopularMovies { get; set; } = new();

    public List<MovieCardViewModel> RecentlyAddedMovies { get; set; } = new();

    public List<MovieCardViewModel> HighestRatedMovies { get; set; } = new();

    public List<GenreSummaryViewModel> Genres { get; set; } = new();
}

public class GenreSummaryViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int MovieCount { get; set; }
}
