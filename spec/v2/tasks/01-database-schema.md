# Oppgave 01: Database Schema - Nye tabeller og kolonner

## Mål
Opprette nye database-tabeller (Users, VideoStatuses, WatchLater) og oppdatere eksisterende Playlist-tabell for v2.

## Kontekst

**Eksisterende v1-schema:**
- `Playlists`: Id, Name, SearchQuery, CreatedUtc, UpdatedUtc
- `CachedSearches`: Id, SearchQuery, ResultsJson, FetchedUtc, ExpiresUtc

**v2-krav:**
- Ny tabell: Users
- Ny tabell: VideoStatuses
- Ny tabell: WatchLater
- Oppdatert tabell: Playlists (nye kolonner)

## Detaljert spesifikasjon

### Ny tabell: Users

```sql
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    CreatedUtc DATETIME2 NOT NULL,
    LastLoginUtc DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

CREATE INDEX IX_Users_Email ON Users(Email);
```

**C# Entity** (skal opprettes i `src/YouTubeKurator.Api/Data/Entities/User.cs`):
```csharp
namespace YouTubeKurator.Api.Data.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public DateTime? LastLoginUtc { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
    public ICollection<WatchLater> WatchLaterItems { get; set; } = new List<WatchLater>();
}
```

### Ny tabell: VideoStatuses

```sql
CREATE TABLE VideoStatuses (
    PlaylistId UNIQUEIDENTIFIER NOT NULL,
    VideoId NVARCHAR(50) NOT NULL,
    Status NVARCHAR(20) NOT NULL, -- New, Seen, Saved, Rejected
    FirstSeenUtc DATETIME2 NOT NULL,
    LastUpdatedUtc DATETIME2 NOT NULL,
    RejectReason NVARCHAR(500) NULL,

    PRIMARY KEY (PlaylistId, VideoId),
    FOREIGN KEY (PlaylistId) REFERENCES Playlists(Id) ON DELETE CASCADE
);

CREATE INDEX IX_VideoStatuses_PlaylistId ON VideoStatuses(PlaylistId);
CREATE INDEX IX_VideoStatuses_Status ON VideoStatuses(Status);
```

**C# Entity** (skal opprettes i `src/YouTubeKurator.Api/Data/Entities/VideoStatus.cs`):
```csharp
namespace YouTubeKurator.Api.Data.Entities;

public class VideoStatus
{
    public Guid PlaylistId { get; set; }
    public string VideoId { get; set; } = string.Empty;
    public VideoStatusEnum Status { get; set; }
    public DateTime FirstSeenUtc { get; set; }
    public DateTime LastUpdatedUtc { get; set; }
    public string? RejectReason { get; set; }

    // Navigation property
    public Playlist Playlist { get; set; } = null!;
}

public enum VideoStatusEnum
{
    New,
    Seen,
    Saved,
    Rejected
}
```

### Ny tabell: WatchLater

```sql
CREATE TABLE WatchLater (
    UserId UNIQUEIDENTIFIER NOT NULL,
    VideoId NVARCHAR(50) NOT NULL,
    PlaylistId UNIQUEIDENTIFIER NULL,
    AddedUtc DATETIME2 NOT NULL,

    PRIMARY KEY (UserId, VideoId, PlaylistId), -- Composite key allows same video in multiple contexts
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (PlaylistId) REFERENCES Playlists(Id) ON DELETE CASCADE
);

CREATE INDEX IX_WatchLater_UserId ON WatchLater(UserId);
CREATE INDEX IX_WatchLater_PlaylistId ON WatchLater(PlaylistId);
```

**C# Entity** (skal opprettes i `src/YouTubeKurator.Api/Data/Entities/WatchLater.cs`):
```csharp
namespace YouTubeKurator.Api.Data.Entities;

public class WatchLater
{
    public Guid UserId { get; set; }
    public string VideoId { get; set; } = string.Empty;
    public Guid? PlaylistId { get; set; }
    public DateTime AddedUtc { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Playlist? Playlist { get; set; }
}
```

### Oppdatert tabell: Playlists

**Nye kolonner:**
```sql
ALTER TABLE Playlists ADD OwnerUserId UNIQUEIDENTIFIER NULL; -- NULL temporarily for migration
ALTER TABLE Playlists ADD Description NVARCHAR(1000) NULL;
ALTER TABLE Playlists ADD Filters NVARCHAR(MAX) NULL; -- JSON
ALTER TABLE Playlists ADD SortStrategy NVARCHAR(50) NULL DEFAULT 'NewestFirst';
ALTER TABLE Playlists ADD DiscoveryProfile NVARCHAR(MAX) NULL; -- JSON
ALTER TABLE Playlists ADD IsPaused BIT NOT NULL DEFAULT 0;

-- Add foreign key constraint (will be populated in migration task 03)
ALTER TABLE Playlists ADD CONSTRAINT FK_Playlists_Users
    FOREIGN KEY (OwnerUserId) REFERENCES Users(Id) ON DELETE CASCADE;

CREATE INDEX IX_Playlists_OwnerUserId ON Playlists(OwnerUserId);
```

**Oppdatert C# Entity** (oppdater eksisterende `src/YouTubeKurator.Api/Data/Entities/Playlist.cs`):
```csharp
namespace YouTubeKurator.Api.Data.Entities;

public class Playlist
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; } // NEW
    public string Name { get; set; } = string.Empty;

    // v1 field - keep for backward compatibility during migration
    public string? SearchQuery { get; set; }

    // v2 fields
    public string? Description { get; set; } // NEW
    public string? Filters { get; set; } // NEW - JSON
    public string SortStrategy { get; set; } = "NewestFirst"; // NEW
    public string? DiscoveryProfile { get; set; } // NEW - JSON
    public bool IsPaused { get; set; } // NEW

    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }

    // Navigation properties
    public User Owner { get; set; } = null!; // NEW
    public ICollection<VideoStatus> VideoStatuses { get; set; } = new List<VideoStatus>(); // NEW
}
```

## DbContext-oppdateringer

**Fil**: `src/YouTubeKurator.Api/Data/AppDbContext.cs`

Oppdater AppDbContext og OnModelCreating med alle nye DbSets og konfigurasjoner. Se planfilen for fullstendig kode.

## EF Core Migration

Generer migrering:
```bash
cd src/YouTubeKurator.Api
dotnet ef migrations add AddV2Schema
dotnet ef database update
```

## Testing

Opprett tester i `YouTubeKurator.Tests/Data/SchemaTests.cs`:
- Test opprettelse av User
- Test opprettelse av VideoStatus
- Test opprettelse av WatchLater
- Test at Playlist har alle nye v2-felt

## Akseptansekriterier

- [ ] Alle 4 nye/oppdaterte entities er opprettet
- [ ] AppDbContext er oppdatert med nye DbSets og konfigurasjoner
- [ ] EF Core migrering er generert og kompilerer
- [ ] Alle schema-tester passerer
- [ ] Database kan opprettes fra scratch (test med SQLite)
- [ ] Ingen breaking changes for eksisterende v1-kode (SearchQuery-felt beholdt)
- [ ] `dotnet build` kompilerer uten feil
- [ ] `dotnet test` passerer alle tester

## Leveranse

- Nye filer: `User.cs`, `VideoStatus.cs`, `WatchLater.cs`
- Oppdatert fil: `Playlist.cs`, `AppDbContext.cs`
- Ny migrasjon: `<timestamp>_AddV2Schema.cs`
- Nye tester: `SchemaTests.cs`
