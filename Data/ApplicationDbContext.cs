using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MovieHub.Models;

namespace MovieHub.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Movie> Movies => Set<Movie>();

    public DbSet<Genre> Genres => Set<Genre>();

    public DbSet<Actor> Actors => Set<Actor>();

    public DbSet<MovieActor> MovieActors => Set<MovieActor>();

    public DbSet<Review> Reviews => Set<Review>();

    public DbSet<Favorite> Favorites => Set<Favorite>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureGenre(builder);
        ConfigureMovie(builder);
        ConfigureActor(builder);
        ConfigureMovieActor(builder);
        ConfigureReview(builder);
        ConfigureFavorite(builder);
    }

    private static void ConfigureGenre(ModelBuilder builder)
    {
        builder.Entity<Genre>(entity =>
        {
            entity.Property(g => g.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(g => g.Name).IsUnique();
        });
    }

    private static void ConfigureMovie(ModelBuilder builder)
    {
        builder.Entity<Movie>(entity =>
        {
            entity.Property(m => m.Title).IsRequired().HasMaxLength(200);
            entity.Property(m => m.Director).IsRequired().HasMaxLength(150);

            // Rating is stored using the SQL Server provider's default mapping for
            // a CLR double (float(53)) — simple and always valid, at the cost of a
            // little floating-point imprecision that doesn't matter for a 0-10 scale.

            // Indexes for frequently searched/filtered/sorted fields.
            entity.HasIndex(m => m.Title);
            entity.HasIndex(m => m.GenreId);
            entity.HasIndex(m => m.ReleaseDate);
            entity.HasIndex(m => m.Rating);

            entity.HasOne(m => m.Genre)
                .WithMany(g => g.Movies)
                .HasForeignKey(m => m.GenreId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureActor(ModelBuilder builder)
    {
        builder.Entity<Actor>(entity =>
        {
            entity.Property(a => a.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(a => a.LastName).IsRequired().HasMaxLength(100);
            entity.HasIndex(a => new { a.LastName, a.FirstName });
        });
    }

    private static void ConfigureMovieActor(ModelBuilder builder)
    {
        builder.Entity<MovieActor>(entity =>
        {
            entity.HasKey(ma => new { ma.MovieId, ma.ActorId });

            entity.HasOne(ma => ma.Movie)
                .WithMany(m => m.MovieActors)
                .HasForeignKey(ma => ma.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ma => ma.Actor)
                .WithMany(a => a.MovieActors)
                .HasForeignKey(ma => ma.ActorId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureReview(ModelBuilder builder)
    {
        builder.Entity<Review>(entity =>
        {
            entity.HasOne(r => r.Movie)
                .WithMany(m => m.Reviews)
                .HasForeignKey(r => r.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // A user may only review a given movie once.
            entity.HasIndex(r => new { r.MovieId, r.UserId }).IsUnique();
        });
    }

    private static void ConfigureFavorite(ModelBuilder builder)
    {
        builder.Entity<Favorite>(entity =>
        {
            entity.HasOne(f => f.Movie)
                .WithMany(m => m.Favorites)
                .HasForeignKey(f => f.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // A user may only favorite a given movie once.
            entity.HasIndex(f => new { f.MovieId, f.UserId }).IsUnique();
        });
    }
}
