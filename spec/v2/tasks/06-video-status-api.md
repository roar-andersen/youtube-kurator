# Oppgave 06: VideoStatus API og Duplikatbeskyttelse

## Mål
Implementere API-endepunkt for å markere videoer som Seen/Saved/Rejected, og duplikatbeskyttelse i refresh.

## Kontekst

**Avhenger av:** Oppgave 01 (VideoStatus-tabell)

**Krav:**
- POST /api/videos/{videoId}/status - Oppdater video-status
- Refresh-endepunkt sjekker VideoStatus og filtrer bort eksisterende videoer
- Status: New, Seen, Saved, Rejected

## Implementering

### 1. VideoStatus Controller

Opprett `src/YouTubeKurator.Api/Controllers/VideoStatusController.cs`:

```csharp
[ApiController]
[Route("api/videos")]
[Authorize]
public class VideoStatusController : ControllerBase
{
    [HttpPost("{videoId}/status")]
    public async Task<IActionResult> UpdateStatus(string videoId, [FromBody] UpdateStatusRequest request)
    {
        // Update or create VideoStatus
        // Return updated status
    }
}

public record UpdateStatusRequest(Guid PlaylistId, string Status, string? RejectReason = null);
```

### 2. VideoStatus Service

Opprett `IVideoStatusService` / `VideoStatusService`:
- `GetStatusAsync(playlistId, videoId): VideoStatus?`
- `UpdateStatusAsync(playlistId, videoId, status, rejectReason): VideoStatus`
- `GetExistingVideoIdsAsync(playlistId): IEnumerable<string>` - For duplikat-sjekk

### 3. Refresh Update

Oppdater `PlaylistsController.Refresh()`:
- Hent alle eksisterende video-IDer for denne playlistens
- Filtrer ut YouTube-resultater som allerede finnes
- Bare returnere videoer med status "New"

### 4. Frontend Integration Points

Dokumenter hvordan frontend skal bruke endepunktet:
- POST /api/videos/{videoId}/status med PlaylistId og status
- UI skal vise status-indikatorer på hver video
- Klikk på knapp skal kalle endepunkt og oppdatere UI

### 5. Testing

Test:
- Opprette ny VideoStatus
- Oppdatere status (New → Seen, Seen → Saved, etc.)
- Duplicate-filtering i refresh
- Permissions (kun innlogget bruker kan markere sine videoer)

## Akseptansekriterier

- [ ] POST /api/videos/{videoId}/status implementert
- [ ] VideoStatus CRUD fungerer
- [ ] Refresh filtrerer bort eksisterende videoer
- [ ] En video kan kun være New én gang per playlist
- [ ] Alle status-transisjoner fungerer
- [ ] Tester passerer
- [ ] Autorisasjon fungerer (kun eier av playlist kan oppdatere)

## Leveranse

Nye filer:
- `VideoStatusController.cs`
- `Services/IVideoStatusService.cs`, `VideoStatusService.cs`
- `VideoStatusServiceTests.cs`

Oppdaterte filer:
- `PlaylistsController.cs` (Refresh-logikk)
