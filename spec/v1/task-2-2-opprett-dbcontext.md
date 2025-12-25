# Oppgave 2.2: Opprett AppDbContext og EF Core-konfigurasjon

## Fase
Fase 2: Datamodell

## Avhengigheter
- Oppgave 2.1 (Playlist-, CachedSearch- og Video-klassene må eksistere)
- Oppgave 1.1 (ASP.NET Core-prosjektet må være opprettet)

## Formål
Sette opp Entity Framework Core-konteksten som administrerer database-kommunikasjon og modell-konfigurering.

## Oppgavebeskrivelse

### 1. Opprett AppDbContext.cs
Lag fil `src/YouTubeKurator.Api/Data/AppDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using YouTubeKurator.Api.Data.Entities;

namespace YouTubeKurator.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<CachedSearch> CachedSearches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Playlist-konfigurasjon
            modelBuilder.Entity<Playlist>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.SearchQuery).IsRequired().HasMaxLength(500);
                entity.Property(e => e.CreatedUtc).IsRequired();
                entity.Property(e => e.UpdatedUtc).IsRequired();
            });

            // CachedSearch-konfigurasjon
            modelBuilder.Entity<CachedSearch>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SearchQuery).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ResultsJson).IsRequired();
                entity.Property(e => e.FetchedUtc).IsRequired();
                entity.Property(e => e.ExpiresUtc).IsRequired();

                // Unikt index på SearchQuery slik at det bare er én cache per søkeord
                entity.HasIndex(e => e.SearchQuery).IsUnique();
            });
        }
    }
}
```

### 2. Registrer DbContext i Program.cs
I `src/YouTubeKurator.Api/Program.cs`, legg til DbContext-registreringen:

```csharp
// Hent connection string fra appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Registrer AppDbContext med SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));
```

Denne koden skal legges til før `var app = builder.Build();`.

### 3. Installer migrations-verktøy (hvis ikke allerede installert)
Sikre at Entity Framework Core Tools er installert:
```bash
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

### 4. Verifiser Database-stien
Sikre at `appsettings.json` inneholder:
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=youtube-kurator.db"
}
```

Denne SQLite-filen vil bli opprettet automatisk når databasen første gang initialiseres.

## Akseptansekriterier
- [ ] `AppDbContext.cs` eksisterer og arver fra `DbContext`
- [ ] `DbSet<Playlist>` og `DbSet<CachedSearch>` er definert
- [ ] `OnModelCreating()` konfigurerer både entiteter korrekt
- [ ] Unikt index er satt på `CachedSearch.SearchQuery`
- [ ] `Program.cs` registrerer `AppDbContext` med SQLite
- [ ] Prosjektet bygges uten feil: `dotnet build`

## Referanser
- [Spesifikasjon: Datamodell](youtube-kurator-v1-spec.md#5-datamodell)
- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
