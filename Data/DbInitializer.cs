using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovieHub.Data.Constants;
using MovieHub.Models;

namespace MovieHub.Data;

/// <summary>
/// Applies pending migrations and seeds the database with roles, a
/// development administrator account, genres, actors and movies.
/// Called once from Program.cs on application startup.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider, IConfiguration configuration, ILogger logger)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager, configuration, logger);
        await SeedGenresAsync(context);
        await SeedActorsAsync(context);
        await SeedMoviesAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var roleName in new[] { Roles.Admin, Roles.User })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger logger)
    {
        var adminEmail = configuration["SeedAdmin:Email"];
        var adminPassword = configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning(
                "SeedAdmin:Email / SeedAdmin:Password are not configured. Skipping development administrator seeding. " +
                "Set them via user-secrets or environment variables to enable this.");
            return;
        }

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is not null)
        {
            return;
        }

        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            DisplayName = "MovieHub Administrator",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            logger.LogInformation("Seeded development administrator account for {Email}.", adminEmail);
        }
        else
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            logger.LogError("Failed to seed administrator account: {Errors}", errors);
        }
    }

    private static async Task SeedGenresAsync(ApplicationDbContext context)
    {
        if (await context.Genres.AnyAsync())
        {
            return;
        }

        var genres = new List<Genre>
        {
            new() { Name = "Action", Description = "High-energy films built around physical stunts, chases and conflict." },
            new() { Name = "Adventure", Description = "Journeys, quests and exploration of exciting, often exotic, settings." },
            new() { Name = "Comedy", Description = "Light-hearted stories designed primarily to amuse and entertain." },
            new() { Name = "Drama", Description = "Character-driven, serious narratives exploring realistic themes." },
            new() { Name = "Horror", Description = "Films designed to frighten, unsettle or thrill through fear." },
            new() { Name = "Sci-Fi", Description = "Speculative stories involving science, technology and the future." },
            new() { Name = "Thriller", Description = "Suspenseful, tense stories that keep audiences on edge." },
            new() { Name = "Romance", Description = "Stories centred on romantic relationships and love." },
            new() { Name = "Animation", Description = "Films created using animated illustration or CGI techniques." },
            new() { Name = "Documentary", Description = "Non-fiction films documenting real events, people or subjects." }
        };

        await context.Genres.AddRangeAsync(genres);
        await context.SaveChangesAsync();
    }

    private static async Task SeedActorsAsync(ApplicationDbContext context)
    {
        if (await context.Actors.AnyAsync())
        {
            return;
        }

        var actors = new List<Actor>
        {
            Actor("Keanu", "Reeves", 1964, 9, 2),
            Actor("Carrie-Anne", "Moss", 1967, 8, 21),
            Actor("Leonardo", "DiCaprio", 1974, 11, 11),
            Actor("Kate", "Winslet", 1975, 10, 5),
            Actor("Tom", "Hanks", 1956, 7, 9),
            Actor("Robin", "Wright", 1966, 4, 8),
            Actor("Morgan", "Freeman", 1937, 6, 1),
            Actor("Christian", "Bale", 1974, 1, 30),
            Actor("Heath", "Ledger", 1979, 4, 4),
            Actor("Scarlett", "Johansson", 1984, 11, 22),
            Actor("Robert", "Downey Jr.", 1965, 4, 4),
            Actor("Chris", "Evans", 1981, 6, 13),
            Actor("Natalie", "Portman", 1981, 6, 9),
            Actor("Brad", "Pitt", 1963, 12, 18),
            Actor("Edward", "Norton", 1969, 8, 18),
            Actor("Tim", "Robbins", 1958, 10, 16),
            Actor("Samuel L.", "Jackson", 1948, 12, 21),
            Actor("Uma", "Thurman", 1970, 4, 29),
            Actor("Matthew", "McConaughey", 1969, 11, 4),
            Actor("Anne", "Hathaway", 1982, 11, 12)
        };

        await context.Actors.AddRangeAsync(actors);
        await context.SaveChangesAsync();
    }

    private static Actor Actor(string firstName, string lastName, int birthYear, int birthMonth, int birthDay) => new()
    {
        FirstName = firstName,
        LastName = lastName,
        BirthDate = new DateTime(birthYear, birthMonth, birthDay, 0, 0, 0, DateTimeKind.Utc),
        Biography = $"{firstName} {lastName} is an accomplished actor known for a wide range of memorable film performances.",
        PhotoUrl = $"https://placehold.co/300x300/1a1a2e/e94560?text={Uri.EscapeDataString(firstName + " " + lastName)}"
    };

    private static async Task SeedMoviesAsync(ApplicationDbContext context)
    {
        if (await context.Movies.AnyAsync())
        {
            return;
        }

        var genres = await context.Genres.ToDictionaryAsync(g => g.Name, g => g.Id);
        var actors = await context.Actors.ToDictionaryAsync(a => a.FirstName + " " + a.LastName, a => a.Id);

        var movies = new List<((Movie Movie, string GenreName) Movie, (string Actor, string Character)[] Cast)>
        {
            (NewMovie("The Matrix", "A hacker discovers reality is a simulation and joins a rebellion against its controllers.", 1999, 3, 31, 136, "Lana Wachowski", "Sci-Fi", 8.7), new[] { ("Keanu Reeves", "Neo"), ("Carrie-Anne Moss", "Trinity") }),
            (NewMovie("Titanic", "A love story unfolds aboard the doomed ocean liner RMS Titanic.", 1997, 12, 19, 195, "James Cameron", "Romance", 7.9), new[] { ("Leonardo DiCaprio", "Jack Dawson"), ("Kate Winslet", "Rose DeWitt Bukater") }),
            (NewMovie("Forrest Gump", "The extraordinary life of a slow-witted but kind-hearted man from Alabama.", 1994, 7, 6, 142, "Robert Zemeckis", "Drama", 8.8), new[] { ("Tom Hanks", "Forrest Gump"), ("Robin Wright", "Jenny Curran") }),
            (NewMovie("The Shawshank Redemption", "Two imprisoned men bond over years, finding solace and redemption.", 1994, 9, 23, 142, "Frank Darabont", "Drama", 9.3), new[] { ("Tim Robbins", "Andy Dufresne"), ("Morgan Freeman", "Ellis 'Red' Redding") }),
            (NewMovie("The Dark Knight", "Batman faces the Joker, a criminal mastermind who plunges Gotham into chaos.", 2008, 7, 18, 152, "Christopher Nolan", "Action", 9.0), new[] { ("Christian Bale", "Bruce Wayne"), ("Heath Ledger", "The Joker") }),
            (NewMovie("Inception", "A thief who steals secrets through dream-sharing is offered a chance at redemption.", 2010, 7, 16, 148, "Christopher Nolan", "Sci-Fi", 8.8), new[] { ("Leonardo DiCaprio", "Dom Cobb"), ("Edward Norton", "Arthur") }),
            (NewMovie("Pulp Fiction", "The lives of two hitmen, a boxer and a gangster's wife intertwine in Los Angeles.", 1994, 10, 14, 154, "Quentin Tarantino", "Thriller", 8.9), new[] { ("Samuel L. Jackson", "Jules Winnfield"), ("Uma Thurman", "Mia Wallace") }),
            (NewMovie("Fight Club", "An insomniac office worker and a soap maker form an underground fight club.", 1999, 10, 15, 139, "David Fincher", "Drama", 8.8), new[] { ("Edward Norton", "The Narrator"), ("Brad Pitt", "Tyler Durden") }),
            (NewMovie("The Avengers", "Earth's mightiest heroes assemble to stop an alien invasion.", 2012, 5, 4, 143, "Joss Whedon", "Action", 8.0), new[] { ("Robert Downey Jr.", "Tony Stark"), ("Chris Evans", "Steve Rogers"), ("Scarlett Johansson", "Natasha Romanoff") }),
            (NewMovie("Iron Man", "A billionaire industrialist builds a powered suit to become the hero Iron Man.", 2008, 5, 2, 126, "Jon Favreau", "Action", 7.9), new[] { ("Robert Downey Jr.", "Tony Stark") }),
            (NewMovie("Captain America: The First Avenger", "A frail soldier becomes a super-soldier to fight the Axis powers.", 2011, 7, 22, 124, "Joe Johnston", "Action", 6.9), new[] { ("Chris Evans", "Steve Rogers") }),
            (NewMovie("Jurassic Park", "Scientists clone dinosaurs to populate an ambitious theme park that goes wrong.", 1993, 6, 11, 127, "Steven Spielberg", "Adventure", 8.2), new[] { ("Samuel L. Jackson", "Ray Arnold") }),
            (NewMovie("La La Land", "An aspiring actress and a jazz musician fall in love in Los Angeles.", 2016, 12, 9, 128, "Damien Chazelle", "Romance", 8.0), new[] { ("Anne Hathaway", "Mia") }),
            (NewMovie("The Notebook", "A poor young man and a rich young woman fall in love in the 1940s South.", 2004, 6, 25, 123, "Nick Cassavetes", "Romance", 7.8), new[] { ("Matthew McConaughey", "Noah Calhoun"), ("Kate Winslet", "Allie Hamilton") }),
            (NewMovie("Interstellar", "A team of explorers travel through a wormhole to ensure humanity's survival.", 2014, 11, 7, 169, "Christopher Nolan", "Sci-Fi", 8.7), new[] { ("Matthew McConaughey", "Cooper"), ("Anne Hathaway", "Brand") }),
            (NewMovie("Gravity", "Two astronauts work together to survive after debris destroys their shuttle.", 2013, 10, 4, 91, "Alfonso Cuaron", "Sci-Fi", 7.7), new[] { ("Scarlett Johansson", "Dr. Ryan Stone") }),
            (NewMovie("Get Out", "A young man uncovers a disturbing secret when he visits his girlfriend's family.", 2017, 2, 24, 104, "Jordan Peele", "Horror", 7.7), new[] { ("Samuel L. Jackson", "Detective Latoya") }),
            (NewMovie("Hereditary", "A family unravels dark secrets after the death of their secretive grandmother.", 2018, 6, 8, 127, "Ari Aster", "Horror", 7.3), new[] { ("Uma Thurman", "Annie Graham") }),
            (NewMovie("A Quiet Place", "A family must live in silence to avoid monsters that hunt by sound.", 2018, 4, 6, 90, "John Krasinski", "Horror", 7.5), new[] { ("Kate Winslet", "Evelyn Abbott") }),
            (NewMovie("The Conjuring", "Paranormal investigators help a family terrorised by a dark presence.", 2013, 7, 19, 112, "James Wan", "Horror", 7.5), new[] { ("Carrie-Anne Moss", "Lorraine Warren") }),
            (NewMovie("Se7en", "Two detectives hunt a serial killer who uses the seven deadly sins as his motives.", 1995, 9, 22, 127, "David Fincher", "Thriller", 8.6), new[] { ("Morgan Freeman", "Detective Somerset"), ("Brad Pitt", "Detective Mills") }),
            (NewMovie("Zodiac", "A cartoonist becomes obsessed with identifying the Zodiac killer.", 2007, 3, 2, 157, "David Fincher", "Thriller", 7.7), new[] { ("Robert Downey Jr.", "Paul Avery") }),
            (NewMovie("Gone Girl", "A man becomes the prime suspect when his wife mysteriously disappears.", 2014, 10, 3, 149, "David Fincher", "Thriller", 8.1), new[] { ("Christian Bale", "Nick Dunne") }),
            (NewMovie("Django Unchained", "A freed slave sets out to rescue his wife from a ruthless plantation owner.", 2012, 12, 25, 165, "Quentin Tarantino", "Action", 8.4), new[] { ("Samuel L. Jackson", "Stephen"), ("Leonardo DiCaprio", "Calvin Candie") }),
            (NewMovie("Toy Story", "A cowboy doll is threatened when a new spaceman action figure arrives.", 1995, 11, 22, 81, "John Lasseter", "Animation", 8.3), new[] { ("Tom Hanks", "Woody (voice)") }),
            (NewMovie("Finding Nemo", "A clownfish searches the ocean for his missing son with a forgetful friend.", 2003, 5, 30, 100, "Andrew Stanton", "Animation", 8.1), new[] { ("Edward Norton", "Additional Voices") }),
            (NewMovie("Shrek", "A grumpy ogre and a talkative donkey set off to rescue a princess.", 2001, 5, 18, 90, "Andrew Adamson", "Animation", 7.9), new[] { ("Chris Evans", "Additional Voices") }),
            (NewMovie("The Grand Budapest Hotel", "A legendary concierge and his loyal lobby boy get caught up in a stolen painting caper.", 2014, 3, 28, 99, "Wes Anderson", "Comedy", 8.1), new[] { ("Edward Norton", "Henckels") }),
            (NewMovie("Superbad", "Two co-dependent high school seniors have one final chance to score alcohol.", 2007, 8, 17, 113, "Greg Mottola", "Comedy", 7.6), new[] { ("Chris Evans", "Additional Cast") }),
            (NewMovie("Free Solo", "A climber attempts to conquer El Capitan without ropes or safety gear.", 2018, 9, 28, 100, "Jimmy Chin", "Documentary", 8.1), Array.Empty<(string, string)>())
        };

        foreach (var ((movie, genreName), cast) in movies)
        {
            movie.GenreId = genres[genreName];
            context.Movies.Add(movie);

            foreach (var (actorName, character) in cast)
            {
                if (actors.TryGetValue(actorName, out var actorId))
                {
                    movie.MovieActors.Add(new MovieActor
                    {
                        ActorId = actorId,
                        CharacterName = character
                    });
                }
            }
        }

        await context.SaveChangesAsync();
    }

    private static (Movie Movie, string GenreName) NewMovie(string title, string description, int year, int month, int day, int duration, string director, string genreName, double rating)
    {
        var movie = new Movie
        {
            Title = title,
            Description = description,
            ReleaseDate = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc),
            Duration = duration,
            Director = director,
            Rating = rating,
            PosterUrl = $"https://placehold.co/400x600/1a1a2e/e94560?text={Uri.EscapeDataString(title)}",
            TrailerUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return (movie, genreName);
    }
}
