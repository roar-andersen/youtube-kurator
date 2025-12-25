# Oppgave 05: Sorteringsstrategier (Backend)

## Mål
Implementere 8 ulike sorteringsstrategier for videoer etter YouTube-fetch.

## Kontekst

**Avhenger av:** Oppgave 03 (Playlist.SortStrategy-felt)

**Strategier (fra spec):**
1. NewestFirst - Publisert tidspunkt (synkende)
2. MostRelevant - YouTube sin score
3. MostPopular - Total visninger (synkende)
4. MostPopularRelative - Views per dag (synkende)
5. HighestQuality - Likes/Views ratio (synkende)
6. LengthShort - Varighet (stigende)
7. LengthLong - Varighet (synkende)
8. ChannelAuthority - Kanal visninger (synkende)
9. WeightedScore - KombinertScore

## Implementering

### 1. Enum

```csharp
public enum SortStrategy
{
    NewestFirst,
    MostRelevant,
    MostPopular,
    MostPopularRelative,
    HighestQuality,
    LengthShort,
    LengthLong,
    ChannelAuthority,
    WeightedScore
}
```

### 2. Sorting Service

Opprett `ISortingService` / `SortingService`:
```csharp
public interface ISortingService
{
    IOrderedEnumerable<Video> Sort(IEnumerable<Video> videos, SortStrategy strategy);
}
```

Implementer hver strategi:
- NewestFirst: `videos.OrderByDescending(v => v.PublishedAt)`
- MostPopular: `videos.OrderByDescending(v => v.ViewCount)`
- HighestQuality: `videos.OrderByDescending(v => v.ViewCount > 0 ? v.LikeCount / (decimal)v.ViewCount : 0)`
- osv.

### 3. YouTube API Updates

Oppdater YouTube-fetch for å inkludere:
- `ViewCount`, `LikeCount`, `CommentCount` fra Videos API
- Kanal-info for "ChannelAuthority"
- Duration for "Length*" strategier

### 4. Playlist Controller

Oppdater `Refresh()`:
- Les `Playlist.SortStrategy`
- Hent videoer
- Appliser sortering før å returnere

### 5. Testing

Test hver strategi med:
- 10+ videoer
- Ulike tidsperioder
- Ulike populæritets-nivåer
- Edge cases (0 views, 0 likes, etc.)

## Akseptansekriterier

- [ ] Alle 8 strategier implementert
- [ ] Sortering fungerer riktig for hver strategi
- [ ] YouTube API returnerer nødvendige felter
- [ ] Refresh bruker valgt strategi
- [ ] Tester passerer alle strategier
- [ ] Edge cases håndteres
- [ ] Performance akseptabel

## Leveranse

Nye filer:
- `Services/ISortingService.cs`, `SortingService.cs`
- `SortingServiceTests.cs`

Oppdaterte filer:
- `YouTubeService.cs`
- `PlaylistsController.cs`
- `Playlist.cs` (enum for SortStrategy)
