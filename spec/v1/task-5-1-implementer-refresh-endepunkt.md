# Oppgave 5.1: Implementer POST /api/playlists/{id}/refresh

## Fase
Fase 5: Refresh-endepunkt

## Avhengigheter
- Oppgave 4.2 (CacheService må være implementert)
- Oppgave 3.1 (PlaylistsController må eksistere)

## Formål
Implementere endepunkt som henter videoer fra YouTube (med cache) når bruker trykker refresh.

## Oppgavebeskrivelse

### 1. Legg til refresh-metode i PlaylistsController
I `src/YouTubeKurator.Api/Controllers/PlaylistsController.cs`, legg til denne metoden:

```csharp
// POST /api/playlists/{id}/refresh
[HttpPost("{id}/refresh")]
public async Task<IActionResult> RefreshPlaylist(Guid id)
{
    try
    {
        // Hent playlist
        var playlist = await _context.Playlists.FindAsync(id);
        if (playlist == null)
        {
            return NotFound(new { error = "Spillelisten ble ikke funnet." });
        }

        // Hent videoer fra cache eller YouTube
        var (videos, fromCache, cacheExpiresUtc, errorType, errorMessage) =
            await _cacheService.GetOrFetchVideosAsync(playlist.SearchQuery);

        // Bygg respons
        var response = new
        {
            videos = videos,
            fromCache = fromCache,
            cacheExpiresUtc = cacheExpiresUtc,
            error = errorType != null ? new { type = errorType, message = errorMessage } : null
        };

        return Ok(response);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new
        {
            videos = new object[0],
            fromCache = false,
            cacheExpiresUtc = (object)null,
            error = new { type = "GenericError", message = "En feil oppstod ved henting av videoer." }
        });
    }
}
```

### 2. Injisér CacheService i PlaylistsController
Oppdater konstruktøren i PlaylistsController:

```csharp
private readonly AppDbContext _context;
private readonly CacheService _cacheService;

public PlaylistsController(AppDbContext context, CacheService cacheService)
{
    _context = context;
    _cacheService = cacheService;
}
```

### 3. Test refresh-endepunktet
Start applikasjonen og test med curl eller Postman:

```bash
# Opprett en playlist først
curl -X POST http://localhost:5000/api/playlists \
  -H "Content-Type: application/json" \
  -d '{"name":"Cooking","searchQuery":"easy recipes"}'

# Hent playlist-ID fra responsen, og bruk den her
curl -X POST http://localhost:5000/api/playlists/{playlist-id}/refresh
```

Forventet respons (ved suksess):
```json
{
  "videos": [
    {
      "videoId": "...",
      "title": "...",
      "channelName": "...",
      "thumbnailUrl": "...",
      "duration": "00:15:30",
      "publishedAt": "2025-01-15T10:00:00Z",
      "viewCount": 1000
    },
    ...
  ],
  "fromCache": false,
  "cacheExpiresUtc": "2025-01-15T11:30:00Z",
  "error": null
}
```

Forventet respons (ved kvota-feil, med tidligere cache):
```json
{
  "videos": [...],
  "fromCache": true,
  "cacheExpiresUtc": "2025-01-15T10:00:00Z",
  "error": {
    "type": "QuotaExceeded",
    "message": "YouTube-kvoten er brukt opp for i dag. Prøv igjen i morgen."
  }
}
```

### 4. Håndter edge cases
Sikre at følgende er håndtert:

- **Playlist ikke funnet**: Returner 404 med passende melding
- **Tomt søkeord**: CacheService håndterer dette
- **YouTube API utilgjengelig**: Returner cached resultat eller feilmelding
- **Ingen internett**: NetworkError returneres
- **Første gang (ingen cache)**: Hent fra YouTube, lagre i cache

## Akseptansekriterier
- [ ] POST /api/playlists/{id}/refresh endepunkt eksisterer
- [ ] Endepunktet henter Playlist fra database
- [ ] Endepunktet kaller CacheService.GetOrFetchVideosAsync()
- [ ] Respons inneholder: videos, fromCache, cacheExpiresUtc, error
- [ ] Ved suksess returneres 200 OK med videoer
- [ ] Ved playlist not found returneres 404
- [ ] Ved YouTube-feil returneres 200 OK med cached data + error-objekt
- [ ] Error-objekt inneholder type og brukervennlig melding

## Referanser
- [Spesifikasjon: Refresh](youtube-kurator-v1-spec.md#refresh)
- [Spesifikasjon: API-endepunkter – Videoer](youtube-kurator-v1-spec.md#videoer)
- [Spesifikasjon: Feilhåndtering](youtube-kurator-v1-spec.md#feilhåndtering)
