# MovieHub

MovieHub is a movie catalog and discovery web application built with ASP.NET Core MVC. Users can browse, search and filter movies, view full movie details (cast, trailer, reviews), rate and review movies, and maintain a personal favorites list. Administrators get a full back-office for managing movies, genres, actors, users and reviews.

This project is intentionally structured as a **learning-friendly, real-world application**: layered (Controllers → Services → EF Core), using view models instead of exposing entities directly where it matters, and following standard ASP.NET Core Identity conventions rather than a bespoke auth system.

## 1. Technology Stack

- ASP.NET Core MVC (.NET 8, the latest LTS at the time of writing)
- C# 12
- Entity Framework Core 8 (SQL Server provider)
- ASP.NET Core Identity (authentication, roles, password hashing)
- Razor Views + Bootstrap 5 (vendored locally, no CDN dependency for the framework itself)
- Bootstrap Icons (CDN)
- jQuery + jQuery Validation / Unobtrusive Validation (vendored locally)
- LINQ, `async`/`await`, Dependency Injection throughout

No SPA framework (React/Angular/Vue/Blazor) is used anywhere — all rendering is server-side Razor.

## 2. Project Structure

```
MovieHub/
├── Controllers/            Home, Movies, Genres, Actors, Reviews, Favorites, Admin, Error
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── DbInitializer.cs    Migration + seed data (roles, admin, genres, actors, movies)
│   └── Constants/Roles.cs  "Admin" / "User" role name constants
├── Models/                 Movie, Genre, Actor, MovieActor, Review, Favorite, ApplicationUser
├── ViewModels/              MovieListViewModel, MovieDetailsViewModel, DashboardViewModel, ...
├── Services/                IMovieService/MovieService, IReviewService/ReviewService, ...
├── Views/                   Razor views, organised by controller, plus Views/Shared partials
├── Areas/Identity/          Only the Register page is overridden (adds Display Name + "User" role);
│                            Login/Logout/Manage/etc. come from the built-in Identity UI library
├── wwwroot/                 css/site.css (dark theme), js/site.js, vendored lib/ (bootstrap, jquery)
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
└── MovieHub.csproj
```

A `MovieHub.sln` file sits alongside the `MovieHub/` project folder so the whole thing opens directly in Visual Studio / Rider / VS Code.

## 3. Main Features

- **Home page** — hero banner, popular / recently added / highest rated rails, genre browser, search bar.
- **Search & filter** (`/Movies`) — search by title, director, actor or genre; filter by genre, year, minimum rating, director; sort (Title A–Z/Z-A, Newest, Oldest, Highest/Lowest rated); paginated (`?page=2&pageSize=12`). All filtering/sorting/paging happens in SQL via LINQ-to-Entities — nothing is pulled into memory and filtered client-side.
- **Movie details** — poster, synopsis, genre, cast (with character names), embedded YouTube trailer (when a trailer URL is set), average user rating, review list, add/remove favorite.
- **Reviews** — authenticated users can post one review per movie (1–10 rating + comment), edit or delete their own review. Admins can delete any review.
- **Favorites** — authenticated users can add/remove movies from a personal favorites list (`/Favorites`), duplicates are prevented at the database level via a unique index.
- **Admin dashboard** (`/Admin`, `Admin` role only) — totals (movies, users, genres, actors, reviews, average rating) plus links into full CRUD for movies/genres/actors and management screens for users (promote/demote Admin, delete account) and reviews (moderate/delete).
- **Authentication** — ASP.NET Core Identity, with `Admin` and `User` roles. New registrations are placed in the `User` role automatically.

## 4. Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (any of the following work):
  - SQL Server LocalDB (ships with Visual Studio, Windows only)
  - SQL Server Developer/Express edition
  - SQL Server in Docker (`mcr.microsoft.com/mssql/server`), for macOS/Linux
- Visual Studio 2022 (17.8+), Rider, or VS Code with the C# Dev Kit — any of these can open `MovieHub.sln`

## 5. SQL Server Setup

**Windows / Visual Studio:** LocalDB is normally already installed; no extra setup needed.

**macOS / Linux (Docker):**

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
  -p 1433:1433 --name moviehub-sql -d mcr.microsoft.com/mssql/server:2022-latest
```

## 6. Connection String Configuration

The default connection string in `appsettings.json` targets LocalDB:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MovieHubDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

If you're using Docker/Express/a remote server instead, override it with **user-secrets** rather than editing `appsettings.json` directly:

```bash
cd MovieHub
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=MovieHubDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
```

In production, set the equivalent environment variable instead: `ConnectionStrings__DefaultConnection`.

## 7. Development Administrator Account

The seeder (`Data/DbInitializer.cs`) creates a development administrator account, but **only if `SeedAdmin:Email` / `SeedAdmin:Password` are configured** — no password is hard-coded into the repository.

`appsettings.Development.json` ships with a convenience default for local development:

- Email: `admin@moviehub.dev`
- Password: `Admin#12345`

This is fine for a throwaway local database, but for anything shared you should override it with user-secrets instead:

```bash
dotnet user-secrets set "SeedAdmin:Email" "admin@yourcompany.dev"
dotnet user-secrets set "SeedAdmin:Password" "SomethingStrongerThanThis1!"
```

In production, leave `SeedAdmin` unset entirely (or set it via environment variables scoped to a one-time bootstrap) — the seeder simply skips admin creation if the values are missing, and logs a warning explaining why.

## 8. Entity Framework Core Migrations

No migrations are checked into the repository on purpose — generate them locally so they match your machine's EF tooling version:

```bash
cd MovieHub
dotnet tool install --global dotnet-ef   # first time only
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
```

`dotnet ef database update` (or simply running the app) creates `MovieHubDb`, all `AspNetUsers`/`AspNetRoles` Identity tables, and the application tables (`Movies`, `Genres`, `Actors`, `MovieActors`, `Reviews`, `Favorites`) with the indexes and relationships configured in `ApplicationDbContext`.

## 9. Seed Data

On first run, `DbInitializer` (invoked from `Program.cs` at startup) automatically:

1. Applies any pending migrations (`Database.MigrateAsync()`).
2. Creates the `Admin` and `User` roles.
3. Creates the development administrator account (see §7).
4. Seeds 10 genres (Action, Adventure, Comedy, Drama, Horror, Sci-Fi, Thriller, Romance, Animation, Documentary).
5. Seeds 20 actors and 30 movies (using real, well-known titles/directors as catalog metadata, paired with placeholder poster images from placehold.co rather than copyrighted artwork) with movie/actor cast relationships.

All seeding is idempotent (`if (await context.Movies.AnyAsync()) return;` etc.) — safe to run the app repeatedly against the same database.

## 10. How to Run the Application

```bash
cd MovieHub
dotnet restore
dotnet ef database update      # after generating a migration, see §8
dotnet run
```

Then browse to the URL shown in the console (typically `https://localhost:5001` or similar — check `Properties/launchSettings.json` for the exact port). Log in with the seeded admin account to access `/Admin`, or register a new account to try the regular user flows (favorites, reviews).

## 11. Project Structure — Where Things Live

| Concern | Location |
|---|---|
| Domain entities | `Models/` |
| EF Core context & seeding | `Data/` |
| Business logic (kept out of controllers so it's reusable from a future API layer) | `Services/` |
| Request/response shaping for views | `ViewModels/` |
| Controllers | `Controllers/` |
| Razor views & shared partials (`_MovieCard`, `_MovieRating`, `_Pagination`) | `Views/` |
| Styling & client scripts | `wwwroot/css/site.css`, `wwwroot/js/site.js` |
| Login/Register/Manage account pages | `Areas/Identity/` (mostly from the built-in Identity UI library; only `Account/Register` is overridden) |

## 12. Main Features Recap

- Browse, search, filter, sort and paginate movies
- Rich movie details with cast, trailer embed, reviews and average rating
- Genres and actors each get their own browsable index/details pages
- Authenticated users: favorites, one review per movie (create/edit/delete own)
- Admins: full CRUD on movies/genres/actors, user role management, review moderation, dashboard stats
- Role-based authorization (`[Authorize(Roles = Roles.Admin)]`), anti-forgery tokens on every mutating form, ownership checks on reviews, no exception details leaked to end users (`ErrorController` + `UseStatusCodePagesWithReExecute`)

## 13. A Note on the Development Environment

This repository was generated and reviewed in a sandboxed environment without outbound access to `nuget.org`, so `dotnet build` could not be executed here to give you a live green build. The code follows standard, well-established ASP.NET Core 8 / EF Core 8 / Identity patterns throughout and was carefully reviewed by hand — package references in `MovieHub.csproj` use floating `8.0.*` versions specifically so restore always resolves to a real, currently-published patch instead of a hard-coded number that might not exist. Still, treat `dotnet restore && dotnet build` as your first real step after cloning, and any compiler errors it surfaces as the actual source of truth over this document.

## 14. Possible Future Improvements

- Promote the `Services/` layer into a proper Web API (the interfaces are already framework-agnostic) to support a mobile app or SPA front-end alongside the MVC site.
- Add integration tests (`WebApplicationFactory`) and unit tests for the service layer.
- Move poster/photo images to actual file storage (Azure Blob Storage / S3) with upload support instead of URL fields.
- Add output caching / response caching for the home page rails and movie listing.
- Support multiple actors "roles" beyond a single character name (e.g. director/writer credits as separate entities).
- Add a "watchlist" separate from favorites, and personalised recommendations.
- Localize the UI (the codebase already keeps user-facing strings in Razor views, ready for resource files).
- Add rate limiting on review/favorite endpoints and CAPTCHA on registration to deter abuse.
- Wire up a real email sender (`IEmailSender`) for account confirmation and password reset in production.
