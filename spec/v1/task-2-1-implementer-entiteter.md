# Oppgave 2.1: Implementer Playlist-entitet og CachedSearch-entitet

## Fase
Fase 2: Datamodell

## Avhengigheter
- Oppgave 1.1 (mappestrukturen må være på plass)

## Formål
Opprett data-entiteter som representerer spillelister og cachede søkeresultater fra YouTube.

## Oppgavebeskrivelse

### 1. Opprett Playlist.cs
Lag fil `src/YouTubeKurator.Api/Data/Entities/Playlist.cs` med følgende struktur:

```csharp
using System;

namespace YouTubeKurator.Api.Data.Entities
{
    public class Playlist
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SearchQuery { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
    }
}
```

Legg til dataannotasjoner/validering:
- `Name`: påkrevd, max 255 tegn
- `SearchQuery`: påkrevd, max 500 tegn
- `Id`: primærnøkkel, GUID
- `CreatedUtc` og `UpdatedUtc`: tidstempel i UTC

Eksempel med attributter:
```csharp
using System.ComponentModel.DataAnnotations;

public class Playlist
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    [Required]
    [StringLength(500)]
    public string SearchQuery { get; set; }

    [Required]
    public DateTime CreatedUtc { get; set; }

    [Required]
    public DateTime UpdatedUtc { get; set; }
}
```

### 2. Opprett CachedSearch.cs
Lag fil `src/YouTubeKurator.Api/Data/Entities/CachedSearch.cs`:

```csharp
using System;
using System.ComponentModel.DataAnnotations;

namespace YouTubeKurator.Api.Data.Entities
{
    public class CachedSearch
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(500)]
        public string SearchQuery { get; set; }

        [Required]
        public string ResultsJson { get; set; }

        [Required]
        public DateTime FetchedUtc { get; set; }

        [Required]
        public DateTime ExpiresUtc { get; set; }
    }
}
```

**Notat**: `ResultsJson` inneholder serialisert JSON av `List<Video>` (se punkt 3).

### 3. Opprett Video.cs (DTO – ikke lagret i database)
Lag fil `src/YouTubeKurator.Api/Data/Entities/Video.cs`:

```csharp
using System;

namespace YouTubeKurator.Api.Data.Entities
{
    public class Video
    {
        public string VideoId { get; set; }
        public string Title { get; set; }
        public string ChannelName { get; set; }
        public string ThumbnailUrl { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime PublishedAt { get; set; }
        public long ViewCount { get; set; }
    }
}
```

### 4. Legg til validering
Sikre at:
- `Playlist.Id` genereres som ny GUID ved opprettelse
- `CreatedUtc` og `UpdatedUtc` settes automatisk (eller i controller)
- `CachedSearch.ExpiresUtc` settes til `FetchedUtc + 1 time`

## Akseptansekriterier
- [ ] `Playlist.cs` eksisterer med alle påkrevde felt og dataannotasjoner
- [ ] `CachedSearch.cs` eksisterer med alle påkrevde felt
- [ ] `Video.cs` eksisterer som DTO
- [ ] Alle klasser er i namespace `YouTubeKurator.Api.Data.Entities`
- [ ] Klassene kan kompileres (burde være ingen build-feil)

## Referanser
- [Spesifikasjon: Datamodell – Playlist](youtube-kurator-v1-spec.md#5-datamodell)
- [Spesifikasjon: Datamodell – CachedSearch](youtube-kurator-v1-spec.md#5-datamodell)
- [Spesifikasjon: Datamodell – Video](youtube-kurator-v1-spec.md#5-datamodell)
