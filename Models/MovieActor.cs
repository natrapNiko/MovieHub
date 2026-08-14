using System.ComponentModel.DataAnnotations;

namespace MovieHub.Models;

/// <summary>
/// Join entity representing the many-to-many relationship between
/// <see cref="Movie"/> and <see cref="Actor"/>, with the character name
/// the actor played in that specific movie.
/// </summary>
public class MovieActor
{
    public int MovieId { get; set; }

    public Movie? Movie { get; set; }

    public int ActorId { get; set; }

    public Actor? Actor { get; set; }

    [StringLength(150)]
    [Display(Name = "Character Name")]
    public string? CharacterName { get; set; }
}
