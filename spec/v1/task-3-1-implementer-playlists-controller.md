# Oppgave 3.1: Implementer PlaylistsController med CRUD-operasjoner

## Fase
Fase 3: API-endepunkter

## Avhengigheter
- Oppgave 2.3 (Database og AppDbContext må være på plass)

## Formål
Implementere REST API-endepunkter for å opprette, lese, oppdatere og slette spillelister.

## Oppgavebeskrivelse

### 1. Opprett PlaylistsController.cs
Lag fil `src/YouTubeKurator.Api/Controllers/PlaylistsController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using YouTubeKurator.Api.Data;
using YouTubeKurator.Api.Data.Entities;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace YouTubeKurator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlaylistsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PlaylistsController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/playlists
        [HttpGet]
        public async Task<IActionResult> GetPlaylists()
        {
            try
            {
                var playlists = await _context.Playlists.ToListAsync();
                return Ok(playlists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "En feil oppstod ved henting av spillelister." });
            }
        }

        // GET /api/playlists/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlaylist(Guid id)
        {
            try
            {
                var playlist = await _context.Playlists.FindAsync(id);
                if (playlist == null)
                {
                    return NotFound(new { error = "Spillelisten ble ikke funnet." });
                }
                return Ok(playlist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "En feil oppstod ved henting av spillelisten." });
            }
        }

        // POST /api/playlists
        [HttpPost]
        public async Task<IActionResult> CreatePlaylist([FromBody] CreatePlaylistRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.SearchQuery))
            {
                return BadRequest(new { error = "Navn og søkeord er påkrevd." });
            }

            try
            {
                var playlist = new Playlist
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    SearchQuery = request.SearchQuery,
                    CreatedUtc = DateTime.UtcNow,
                    UpdatedUtc = DateTime.UtcNow
                };

                _context.Playlists.Add(playlist);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPlaylist), new { id = playlist.Id }, playlist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "En feil oppstod ved opprettelse av spillelisten." });
            }
        }

        // PUT /api/playlists/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlaylist(Guid id, [FromBody] UpdatePlaylistRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.SearchQuery))
            {
                return BadRequest(new { error = "Navn og søkeord er påkrevd." });
            }

            try
            {
                var playlist = await _context.Playlists.FindAsync(id);
                if (playlist == null)
                {
                    return NotFound(new { error = "Spillelisten ble ikke funnet." });
                }

                playlist.Name = request.Name;
                playlist.SearchQuery = request.SearchQuery;
                playlist.UpdatedUtc = DateTime.UtcNow;

                _context.Playlists.Update(playlist);
                await _context.SaveChangesAsync();

                return Ok(playlist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "En feil oppstod ved oppdatering av spillelisten." });
            }
        }

        // DELETE /api/playlists/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlaylist(Guid id)
        {
            try
            {
                var playlist = await _context.Playlists.FindAsync(id);
                if (playlist == null)
                {
                    return NotFound(new { error = "Spillelisten ble ikke funnet." });
                }

                _context.Playlists.Remove(playlist);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "En feil oppstod ved sletting av spillelisten." });
            }
        }
    }
}
```

### 2. Opprett Request/Response-klasser
Lag fil `src/YouTubeKurator.Api/Controllers/PlaylistRequests.cs`:

```csharp
namespace YouTubeKurator.Api.Controllers
{
    public class CreatePlaylistRequest
    {
        public string Name { get; set; }
        public string SearchQuery { get; set; }
    }

    public class UpdatePlaylistRequest
    {
        public string Name { get; set; }
        public string SearchQuery { get; set; }
    }
}
```

### 3. Verifiser at API-endepunktene er registrert
I `Program.cs`, sikre at controllers er registrert:

```csharp
builder.Services.AddControllers();
```

Og i app-konfigureringen:

```csharp
app.MapControllers();
```

### 4. Test endepunktene lokalt
Start applikasjonen:
```bash
dotnet run
```

Bruk f.eks. Postman, curl eller VS Code REST Client for å teste:

**GET /api/playlists**
```bash
curl http://localhost:5000/api/playlists
```

**POST /api/playlists**
```bash
curl -X POST http://localhost:5000/api/playlists \
  -H "Content-Type: application/json" \
  -d '{"name":"Musik","searchQuery":"best songs 2025"}'
```

**PUT /api/playlists/{id}**
```bash
curl -X PUT http://localhost:5000/api/playlists/{id} \
  -H "Content-Type: application/json" \
  -d '{"name":"Musik - Updated","searchQuery":"best songs 2024"}'
```

**DELETE /api/playlists/{id}**
```bash
curl -X DELETE http://localhost:5000/api/playlists/{id}
```

## Akseptansekriterier
- [ ] PlaylistsController.cs eksisterer i `Controllers/`-mappen
- [ ] GET /api/playlists returnerer liste av alle spillelister
- [ ] POST /api/playlists opprettter ny spilleliste og returnerer 201 Created
- [ ] GET /api/playlists/{id} returnerer enkelt playlist eller 404
- [ ] PUT /api/playlists/{id} oppdaterer playlist og returnerer 200 OK
- [ ] DELETE /api/playlists/{id} sletter playlist og returnerer 204 No Content
- [ ] Validering av påkrevde felt fungerer (400 Bad Request ved missing data)
- [ ] Alle endepunkter håndterer exceptions og returnerer 500 ved feil

## Referanser
- [Spesifikasjon: API-endepunkter – Spillelister](youtube-kurator-v1-spec.md#7-api-endepunkter)
- [Microsoft ASP.NET Core Controllers](https://learn.microsoft.com/en-us/aspnet/core/web-api/)
