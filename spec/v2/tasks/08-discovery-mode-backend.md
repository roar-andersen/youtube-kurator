# Oppgave 08: Oppdagelsesmodus (Backend)

## Mål
Implementere oppdagelsesmodus som blander streng filtrering med villkort (wild cards).

## Kontekst

**Avhenger av:** Oppgave 04 og 05 (Filtre og sortering)

**Fra spec:**
- 70% strengt matchende (følger alle filtre)
- 20% bryter ett kriterium svakt (relaxed)
- 10% villkort (wild cards)

**Villkort-typer:**
- Samme tema, annet språk
- Samme kanal, annet format
- Lav popularitet, høy kvalitet
- Relaterte kanaler

## Discovery Profile JSON

```json
{
  "strict": 70,
  "relaxed": 20,
  "wild": 10,
  "enableWildcards": true,
  "wildcardTypes": [
    "sametheme_otherlangage",
    "samechannel_otherformat",
    "lowpop_highquality",
    "relatedchannels"
  ]
}
```

## Implementering

### 1. Discovery Models

Opprett models for discovery configuration.

### 2. Discovery Service

Opprett `IDiscoveryService` / `DiscoveryService`:
- `SelectVideosWithDiscoveryAsync(candidates, playlist, count)`
- `ApplyDiscoveryLogicAsync(videos, discoveryProfile)`
- `ExplainSelectionAsync(videoId)` - Forklar hvorfor video ble valgt

Logikk:
1. Hent 3x antall ønskede videoer som kandidater
2. Del dem i 3 grupper (70% strict, 20% relaxed, 10% wild)
3. For strict: bruk alle filtre
4. For relaxed: relaks 1 kriterium
5. For wild: hent fra YouTube Related Videos API
6. Bland gruppene tilbake til enkel liste
7. Sorter basert på strategy
8. Returner ønsket antall

### 3. Related Videos Service

Implementer YouTube Related Videos API-kall:
- `GetRelatedVideosAsync(videoId): IEnumerable<Video>`
- Cachet resultater for å spare kvota
- Håndter API-feil

### 4. Refresh Update

Oppdater refresh for å inkludere discovery:
- Les `Playlist.DiscoveryProfile`
- Appliser oppdagelsesmodus hvis enabled
- Returner blandet liste

### 5. Video Selection Explanation

I video-objektet, legg til field for å forklare valg:
```csharp
public class Video
{
    // ... eksisterende felt ...
    public string? DiscoveryReason { get; set; } // "Strict match", "Related channel", etc.
}
```

### 6. Testing

Test:
- Blanding av strict/relaxed/wild (ratio)
- Explanation-tekster
- Related Videos API-kall
- Caching
- Edge cases (færre kandidater enn ønsket)

## Akseptansekriterier

- [ ] Discovery profile JSON lagres og lastes
- [ ] 70/20/10-split fungerer
- [ ] Wild cards hentes fra Related Videos API
- [ ] Explanation fungerer og returneres til frontend
- [ ] Caching fungerer for Related Videos
- [ ] Tester passerer
- [ ] Refresh bruker discovery

## Leveranse

Nye filer:
- `Models/DiscoveryProfile.cs`
- `Services/IDiscoveryService.cs`, `DiscoveryService.cs`
- `Services/IRelatedVideosService.cs`, `RelatedVideosService.cs`
- `DiscoveryServiceTests.cs`

Oppdaterte filer:
- `PlaylistsController.cs` (Refresh-logikk)
- `Video.cs` (DiscoveryReason-felt)
