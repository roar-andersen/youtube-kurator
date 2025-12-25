# Oppgave 07: Watch Later API

## Mål
Implementere API for "Se på senere" funksjonalitet - global per bruker og spilleliste-spesifikk.

## Kontekst

**Avhenger av:** Oppgave 01 (WatchLater-tabell)

**Krav:**
- GET /api/watchlater - Hent alle watch later videoer for bruker
- POST /api/watchlater - Legg til video til watch later
- DELETE /api/watchlater/{videoId} - Fjern video fra watch later

**Funksjonalitet:**
- Global watch later når PlaylistId = null
- Spilleliste-spesifikk når PlaylistId er satt
- Samme video kan være i flere lister

## Implementering

### 1. WatchLater Controller

Opprett `src/YouTubeKurator.Api/Controllers/WatchLaterController.cs`:

```csharp
[ApiController]
[Route("api/watchlater")]
[Authorize]
public class WatchLaterController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? playlistId = null)
    {
        // Return all watch later videos for user
        // If playlistId: return only for that playlist
        // If no playlistId: return global watch later
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddWatchLaterRequest request)
    {
        // Add video to watch later
    }

    [HttpDelete("{videoId}")]
    public async Task<IActionResult> Remove(string videoId, [FromQuery] Guid? playlistId = null)
    {
        // Remove video from watch later
    }
}

public record AddWatchLaterRequest(string VideoId, Guid? PlaylistId = null);
```

### 2. WatchLater Service

Opprett `IWatchLaterService` / `WatchLaterService`:
- `GetWatchLaterAsync(userId, playlistId?): IEnumerable<WatchLater>`
- `AddAsync(userId, videoId, playlistId?): WatchLater`
- `RemoveAsync(userId, videoId, playlistId?): bool`
- `IsInWatchLaterAsync(userId, videoId): bool` - For UI-indikatorer

### 3. Testing

Test:
- Legg til video (global og playlist-spesifikk)
- Hent alle videoer
- Fjern video
- Samme video i flere playlists
- Permissions (bruker kan bare se egne watch later)

### 4. Frontend Integration Points

Dokumenter:
- GET /api/watchlater - Hent global watch later liste
- GET /api/watchlater?playlistId=xyz - Hent playlist-spesifikk
- POST /api/watchlater med VideoId og optional PlaylistId
- DELETE /api/watchlater/{videoId}?playlistId=xyz
- UI skal vise watch later-ikon på videoer som allerede er i watch later

## Akseptansekriterier

- [ ] GET /api/watchlater implementert
- [ ] POST /api/watchlater implementert
- [ ] DELETE /api/watchlater/{videoId} implementert
- [ ] Global og playlist-spesifikk watch later fungerer
- [ ] Samme video kan være i flere lister
- [ ] Tester passerer
- [ ] Autorisasjon fungerer

## Leveranse

Nye filer:
- `WatchLaterController.cs`
- `Services/IWatchLaterService.cs`, `WatchLaterService.cs`
- `WatchLaterServiceTests.cs`

Oppdaterte filer:
- `Program.cs` (registrer service)
