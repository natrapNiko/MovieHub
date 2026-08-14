namespace MovieHub.ViewModels;

//Lightweight paging metadata shared by any list view that needs a pager
//(movies, actors, genres, admin lists, ...). Kept independent of the
//item type so the same _Pagination partial can render any of them.

public class PaginationViewModel
{
    public int CurrentPage { get; set; }

    public int PageSize { get; set; }

    public int TotalItems { get; set; }

    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => CurrentPage < TotalPages;

    //The controller action the pager links back to, e.g. "Index".
    public string Action { get; set; } = "Index";
}
