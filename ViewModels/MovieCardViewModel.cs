namespace MovieHub.ViewModels;

//Projection used everywhere a movie is rendered as a card (home page
//rails, search results, favorites list) so we never pull more data
//from the database than the view actually needs.
public class MovieCardViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public int ReleaseYear { get; set; }

    public string GenreName { get; set; } = string.Empty;

    public double AverageRating { get; set; }

    public int ReviewCount { get; set; }

    public string? PosterUrl { get; set; }

    public string ShortDescription { get; set; } = string.Empty;
}
