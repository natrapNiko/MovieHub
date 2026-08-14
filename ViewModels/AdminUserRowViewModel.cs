namespace MovieHub.ViewModels;

public class AdminUserRowViewModel
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public bool IsAdmin { get; set; }
}
