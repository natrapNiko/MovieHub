using System.ComponentModel.DataAnnotations;

namespace MovieHub.ViewModels;

public class ActorFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(100)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(100)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Biography { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime? BirthDate { get; set; }

    [StringLength(500)]
    [Url(ErrorMessage = "Please provide a valid URL.")]
    [Display(Name = "Photo URL")]
    public string? PhotoUrl { get; set; }
}
