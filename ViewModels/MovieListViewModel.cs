using System.ComponentModel.DataAnnotations;

namespace MovieHub.ViewModels;

public enum MovieSortOrder
{
    [Display(Name = "Title A-Z")]
    TitleAsc,

    [Display(Name = "Title Z-A")]
    TitleDesc,

    [Display(Name = "Newest First")]
    Newest,

    [Display(Name = "Oldest First")]
    Oldest,

    [Display(Name = "Highest Rated")]
    HighestRated,

    [Display(Name = "Lowest Rated")]
    LowestRated
}

//Backs /Movies (search, filter, sort and pagination combined).
public class MovieListViewModel
{
    public List<MovieCardViewModel> Movies { get; set; } = new();

    public PaginationViewModel Pagination { get; set; } = new();

    // Filters/search echoed back into the form so the UI reflects the current query.
    public string? Search { get; set; }

    public int? GenreId { get; set; }

    public int? Year { get; set; }

    public double? MinRating { get; set; }

    public string? Director { get; set; }

    public MovieSortOrder Sort { get; set; } = MovieSortOrder.Newest;

    public List<GenreSummaryViewModel> AvailableGenres { get; set; } = new();

    public List<int> AvailableYears { get; set; } = new();
}
