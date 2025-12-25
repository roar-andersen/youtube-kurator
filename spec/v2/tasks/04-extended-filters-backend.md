# Oppgave 04: Utvidede Filtre (Backend)

## Mål
Implementere backend-logikk for avansert filtrering av YouTube-videoer basert på JSON-konfigurert filters.

## Kontekst

**Avhenger av:** Oppgave 03 (Playlist-struktur med Filters-felt)

**Fra v2-spec:**
- Temaer (fritekst, søkeord)
- Varighet (min/maks sekunder)
- Publiseringstid (absolutt dato eller relativ periode)
- Språk og region
- Innholdstype (video/livestream/shorts)
- Popularitet/kvalitet (min visninger, likes, ratio)
- Kanal-filter (inkluder/ekskluder kanaler)

## Filter JSON Schema

```json
{
  "themes": ["gaming", "tutorial"],
  "includeKeywords": ["unity", "game engine"],
  "excludeKeywords": ["tutorial"],
  "duration": {
    "minSeconds": 60,
    "maxSeconds": 3600
  },
  "publishedTime": {
    "type": "relative",
    "days": 30
  },
  "language": {
    "preferred": "no",
    "region": "NO"
  },
  "contentType": {
    "videos": true,
    "livestreams": false,
    "shorts": false
  },
  "popularity": {
    "minViews": 1000,
    "minLikes": 100,
    "minLikeRatio": 0.02
  },
  "channels": {
    "include": ["UC_channel_id_1"],
    "exclude": []
  }
}
```

## Implementering

### 1. Filter Models

Opprett `src/YouTubeKurator.Api/Models/PlaylistFilters.cs` med klasser som matcher JSON-strukturen.

### 2. Filter Service

Opprett `IFilterService` / `FilterService`:
- `EvaluateFilters(video, filters): bool` - Sjekker om video matcher alle filtre
- `NormalizeVideoData(youtubeVideo): Video` - Konverterer YouTube-data til format egnet for filtering

### 3. YouTube API Integration

Oppdater `YouTubeService`:
- Hent `duration`, `publishedAt`, `statistics` fra YouTube Videos API
- Implementer `GetChannelInfo()` for å sjekke kanal-filter
- Cach kanal-data for å ikke overforbruke kvota

### 4. Refresh Endpoint Update

Oppdater `PlaylistsController.Refresh()`:
- Parse Playlist.Filters JSON til FilterModel
- Hent kandidater fra YouTube
- Filtrere bort videoer som ikke matcher filtre
- Filtrere bort videoer fra VideoStatus (duplikater)
- Sorter basert på SortStrategy
- Returner max 20 nye videoer

### 5. Testing

Test:
- Alle filtertyper enkeltvis
- Kombinert filtrering
- YouTube API-kall
- Filtering av eksisterende videoer

## Akseptansekriterier

- [ ] Alle 7 filtertyper implementert
- [ ] Filters JSON lagres og lastes riktig
- [ ] YouTube API henter alle nødvendige felt
- [ ] Videoer filtres riktig på alle kriteria
- [ ] Refresh-endepunkt bruker filtre
- [ ] Tester passerer
- [ ] Ytelse akseptabel (not N+1 queries)
- [ ] Duplikater filtreres bort

## Leveranse

Nye filer:
- `Models/PlaylistFilters.cs`
- `Services/IFilterService.cs`, `FilterService.cs`
- `FilterServiceTests.cs`

Oppdaterte filer:
- `YouTubeService.cs`
- `PlaylistsController.cs`
