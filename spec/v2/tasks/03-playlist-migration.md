# Oppgave 03: Playlist-migrering (v1 → v2)

## Mål
Migrere alle eksisterende v1-spillelister til v2-skjemaet uten å miste data.

## Kontekst

**Avhenger av:** Oppgave 01 og 02 (User-tabell og Playlist-kolonner må finnes)

**Utfordringer:**
- Eksisterende playlists har ingen OwnerUserId (må lages standard-bruker)
- SearchQuery må konverteres til Filters (JSON)
- Nye felter (Description, SortStrategy, DiscoveryProfile, IsPaused) må få default-verdier

**Løsning:**
1. Opprett en standard-bruker (system user) for alle eksisterende playlists
2. Konverter SearchQuery → Filters JSON format
3. Sett default-verdier for nye felter

## Implementering

### 1. Migration Script

Opprett `src/YouTubeKurator.Api/Data/Migrations/<timestamp>_MigrateV1ToV2.cs`:

```csharp
using Microsoft.EntityFrameworkCore.Migrations;

namespace YouTubeKurator.Api.Data.Migrations;

public partial class MigrateV1ToV2 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create system user for existing playlists
        var systemUserId = Guid.NewGuid();
        migrationBuilder.InsertData(
            table: "Users",
            columns: new[] { "Id", "Email", "CreatedUtc", "IsActive" },
            values: new object[] { systemUserId, "system@youtube-kurator.local", DateTime.UtcNow, true }
        );

        // Update all playlists with system user and convert SearchQuery to Filters
        migrationBuilder.Sql($@"
            UPDATE Playlists
            SET
                OwnerUserId = '{systemUserId}',
                Filters = json('{{""themes"": [""' + REPLACE(SearchQuery, ' ', '"", ""') + '""]}}'),
                SortStrategy = 'NewestFirst',
                IsPaused = 0
            WHERE OwnerUserId IS NULL
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("UPDATE Playlists SET OwnerUserId = NULL WHERE Email = 'system@youtube-kurator.local'");
        migrationBuilder.DeleteData(table: "Users", keyColumn: "Email", keyValue: "system@youtube-kurator.local");
    }
}
```

### 2. Playlist-kontrollerens oppdatering

Oppdater `PlaylistsController` slik at:
- V1-API (`GET /api/playlists`, `POST /api/playlists`, osv.) fortsatt fungerer uten auth
- V1-brukere behandles som "system user"
- V2-API krever JWT-token
- Gradvis migrering: system user kan bruke både v1 og v2 API

**Implementering:**
```csharp
[ApiController]
[Route("api/playlists")]
public class PlaylistsController : ControllerBase
{
    // Eksisterende v1-metoder fungerer fortsatt
    // Nye v2-metoder krever [Authorize]

    // Hjelpefunksjon for å få bruker (v1: system user, v2: JWT claims)
    private async Task<Guid> GetUserIdAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;

        // v1 kompatibilitet: bruk system user
        return await _context.Users
            .Where(u => u.Email == "system@youtube-kurator.local")
            .Select(u => u.Id)
            .FirstOrDefaultAsync();
    }
}
```

### 3. Data Seeding

Oppdater `AppDbContext` for å seede system-brukeren hvis den ikke finnes:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // ... eksisterende config ...

    // Seed system user
    modelBuilder.Entity<User>().HasData(
        new User
        {
            Id = new Guid("00000000-0000-0000-0000-000000000001"),
            Email = "system@youtube-kurator.local",
            CreatedUtc = DateTime.UtcNow,
            IsActive = true
        }
    );
}
```

### 4. Verktøy for manuell migrering (hvis migration feiler)

Opprett `src/YouTubeKurator.Api/Tools/PlaylistMigrationTool.cs`:

```csharp
public class PlaylistMigrationTool
{
    public static async Task MigrateAllPlaylistsAsync(AppDbContext context)
    {
        var systemUser = await context.Users
            .FirstOrDefaultAsync(u => u.Email == "system@youtube-kurator.local");

        if (systemUser == null)
        {
            systemUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "system@youtube-kurator.local",
                CreatedUtc = DateTime.UtcNow,
                IsActive = true
            };
            context.Users.Add(systemUser);
            await context.SaveChangesAsync();
        }

        var playlistsToMigrate = await context.Playlists
            .Where(p => p.OwnerUserId == Guid.Empty || p.OwnerUserId == null)
            .ToListAsync();

        foreach (var playlist in playlistsToMigrate)
        {
            playlist.OwnerUserId = systemUser.Id;

            // Convert SearchQuery to Filters JSON
            if (!string.IsNullOrEmpty(playlist.SearchQuery))
            {
                var filters = new
                {
                    themes = new[] { playlist.SearchQuery }
                };
                playlist.Filters = JsonSerializer.Serialize(filters);
            }

            playlist.SortStrategy ??= "NewestFirst";
            playlist.IsPaused = false;
        }

        await context.SaveChangesAsync();
    }
}
```

### 5. Testing

Opprett `YouTubeKurator.Tests/Data/PlaylistMigrationTests.cs`:
- `Migrate_PlayllistsGetOwnerUserId`
- `Migrate_SearchQueryConvertedToFilters`
- `Migrate_DefaultValuesSet`
- `Migrate_DataPreserved`
- Test at v1-API fortsatt fungerer etter migrering

## Migreringsstrategi

**Fase 1: Forberedelse**
- Databasen has både v1 og v2 struktur
- System-bruker er opprettet

**Fase 2: Gradvis migrering**
- V1-API fungerer som før (bruker system-bruker)
- V2-API er åpen (krever JWT)
- Playlists er migrert, men fungerer med system-bruker

**Fase 3: Sluttfase**
- Brukere logger inn med v2
- Deres playlists vises under deres bruker
- System-bruker blir arkivert/slettet

## Akseptansekriterier

- [ ] Alle eksisterende playlists får OwnerUserId
- [ ] SearchQuery konverteres til Filters JSON
- [ ] Alle nye felter får riktige default-verdier
- [ ] Ingen data går tapt
- [ ] V1-API fungerer fortsatt
- [ ] System-bruker kan ses i database
- [ ] Migrering kan kjøres og rulle tilbake
- [ ] Tester passerer
- [ ] `dotnet build` kompilerer

## Leveranse

Nye filer:
- `PlaylistMigrationTool.cs` (valgfritt, for fallback)

Oppdaterte filer:
- `PlaylistsController.cs`
- `AppDbContext.cs`

Nye migrasjoner:
- `<timestamp>_MigrateV1ToV2.cs`

Nye tester:
- `PlaylistMigrationTests.cs`
