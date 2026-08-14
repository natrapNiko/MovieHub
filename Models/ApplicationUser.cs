using Microsoft.AspNetCore.Identity;

namespace MovieHub.Models;

//Application user extending ASP.NET Core Identity's IdentityUser with
//profile fields used throughout MovieHub (e.g. display name on reviews).
public class ApplicationUser : IdentityUser
{
    [PersonalData]
    public string DisplayName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}
