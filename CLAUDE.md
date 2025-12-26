# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

YouTube Kurator is a Progressive Web App (PWA) for creating and managing theme-based YouTube playlists with automatic video discovery. Built with ASP.NET Core 10.0 backend and Alpine.js frontend, it features JWT authentication, smart caching, video filtering/sorting, and discovery mode.

**Tech Stack:**
- Backend: ASP.NET Core 10.0 Web API
- Frontend: Vanilla JavaScript with Alpine.js 3.x
- Database: SQLite (dev), Azure SQL (prod)
- ORM: Entity Framework Core 10.0.1
- API: YouTube Data API v3
- Testing: xUnit, Moq
- Containerization: Docker
- Hosting: Azure Container Apps

## Essential Commands

### Build and Run

```bash
# Restore dependencies
cd YouTubeKurator.Api
dotnet restore

# Run application (development)
dotnet run
# Runs on http://localhost:5228 and https://localhost:7004

# Build for production
dotnet build -c Release

# Clean build artifacts
dotnet clean
```

### Database Operations

```bash
# Apply migrations (creates SQLite database)
cd YouTubeKurator.Api
dotnet ef database update

# Create new migration
dotnet ef migrations add MigrationName

# View database context info
dotnet ef dbcontext info
```

### Testing

```bash
# Run all tests
dotnet test YouTubeKurator.Tests/YouTubeKurator.Tests.csproj

# Run with timeout (useful for hung tests)
timeout 60 dotnet test YouTubeKurator.Tests/YouTubeKurator.Tests.csproj

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test
dotnet test --filter "FullyQualifiedName~TestMethodName"
```

### Docker

```bash
# Build image
docker build -t youtube-kurator:latest .

# Run container
docker run -p 8080:80 \
  -e "YouTubeApi__ApiKey=YOUR_API_KEY" \
  -e "ConnectionStrings__DefaultConnection=Data Source=youtube-kurator.db" \
  youtube-kurator:latest
```

## Architecture

### High-Level Structure

The application follows a layered architecture:

```
Controllers → Services → Data (EF Core) → Database
     ↓
  wwwroot (Alpine.js PWA)
```

**Key Layers:**

1. **Controllers** (`Controllers/`): REST API endpoints
   - `PlaylistsController`: CRUD operations + refresh endpoint
   - `AuthController`: Magic link authentication (send code, verify code)
   - `VideoStatusController`: Mark videos as watched/rejected
   - `WatchLaterController`: Watch later functionality

2. **Services** (`Services/`): Business logic
   - `YouTubeService`: YouTube API integration (search, video details)
   - `CacheService`: 1-hour caching for YouTube searches
   - `FilterService`: Video filtering (duration, upload date, view count, language, channel)
   - `SortingService`: Multiple sort strategies (newest, oldest, most viewed, etc.)
   - `DiscoveryService`: Discovery mode with related videos and diversity scoring
   - `RelatedVideosService`: Fetches YouTube related videos
   - `VideoStatusService`: Manages video status (watched, rejected)
   - `WatchLaterService`: Watch later management
   - `AuthService`: Authentication logic (magic link)
   - `JwtService`: JWT token generation/validation
   - `EmailService`: Email sending (currently logs to console)

3. **Data Layer** (`Data/`):
   - `AppDbContext`: EF Core DbContext with all entity configurations
   - `Entities/`: Database models (User, Playlist, VideoStatus, WatchLater, AuthCode, CachedSearch)

4. **Frontend** (`wwwroot/`):
   - `index.html`: Single-page app structure
   - `app.js`: Alpine.js application with state management
   - `styles.css`: Mobile-first responsive styling
   - `manifest.json` + `sw.js`: PWA support

### Authentication & Authorization

- **Magic Link Flow**: User enters email → receives 6-digit code → verifies code → receives JWT token
- **JWT Configuration**: Tokens use HMAC SHA256, configured in `Program.cs:66-91`
- **System User**: Default user (`00000000-0000-0000-0000-000000000001`) seeded for v1 compatibility (AppDbContext.cs:46-54)
- **Authorization**: All playlist endpoints require `[Authorize]` attribute and validate user ownership

### Multi-User Architecture

- Each playlist is owned by a specific user (`OwnerUserId` foreign key)
- Users can only access their own playlists (enforced in controller)
- VideoStatus and WatchLater are tied to users
- Cross-device sync supported through user accounts

### Caching Strategy

**Implementation**: `CacheService.cs`
- **Cache Duration**: 1 hour per YouTube search query
- **Cache Key**: Search query string (case-sensitive)
- **Storage**: Database table `CachedSearches` (JSON serialized)
- **Invalidation**: Automatic after 1 hour (`ExpiresUtc`)
- **Quota Management**: Serves cached results when YouTube API quota exceeded

**Important**: Cache is keyed by search query only, not by playlist ID. Multiple playlists with same search query share cache.

### Video Filtering & Sorting

**Filters** (`PlaylistFilters` model):
- Duration: `Any`, `Short` (<4 min), `Medium` (4-20 min), `Long` (>20 min)
- Upload date: `Any`, `LastHour`, `Today`, `ThisWeek`, `ThisMonth`, `ThisYear`
- View count: minimum threshold
- Language: ISO 639-1 codes (e.g., "en", "no")
- Channel names: comma-separated inclusion/exclusion lists

**Sorting** (`SortStrategy` enum):
- `NewestFirst`, `OldestFirst`, `MostViewed`, `LeastViewed`, `ShortestFirst`, `LongestFirst`, `Relevance`

Filters and sort strategy are stored as JSON/string on Playlist entity and applied during refresh.

### Discovery Mode

**Purpose**: Introduce serendipity by diversifying video sources beyond the initial search query.

**Flow** (`DiscoveryService.cs`):
1. Check if playlist has `DiscoveryProfile` enabled
2. Select up to 3 "seed videos" from filtered results
3. Fetch related videos for each seed (up to 10 per seed)
4. Score all videos by diversity (channel diversity, topic diversity)
5. Mix 60% standard results + 40% discovery results
6. Return combined, deduplicated list

**Configuration**: Enable/disable via `UpdatePlaylistRequest.EnableDiscovery`

### Database Schema

**Key Entities**:

- **User**: `Id` (PK), `Email` (unique), `CreatedUtc`, `IsActive`
- **Playlist**: `Id` (PK), `Name`, `SearchQuery`, `OwnerUserId` (FK to User), `Filters` (JSON), `SortStrategy`, `DiscoveryProfile` (JSON), `IsPaused`, timestamps
- **VideoStatus**: Composite PK (`PlaylistId`, `VideoId`), `Status` (enum: Unseen, Watched, Rejected), `FirstSeenUtc`, `LastUpdatedUtc`, `RejectReason`
- **WatchLater**: Composite PK (`UserId`, `VideoId`, `PlaylistId`), `AddedUtc`
  - `PlaylistId = Guid.Empty` represents global watch later (no specific playlist)
- **AuthCode**: `Id` (PK), `Email`, `Code`, `CreatedUtc`, `ExpiresUtc`, `IsUsed`
- **CachedSearch**: `Id` (PK), `SearchQuery` (unique), `ResultsJson`, `FetchedUtc`, `ExpiresUtc`

**Relationships**:
- User → Playlists (1:many, cascade delete)
- User → WatchLater (1:many, cascade delete)
- Playlist → VideoStatuses (1:many, cascade delete)

### Configuration

**Required Settings** (`appsettings.Development.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=youtube-kurator.db"
  },
  "YouTubeApi": {
    "ApiKey": "YOUR_YOUTUBE_API_KEY_HERE"
  },
  "Jwt": {
    "SecretKey": "your-32-character-secret-key-here",
    "Issuer": "YouTubeKurator",
    "Audience": "YouTubeKurator"
  }
}
```

**YouTube API Key**: Get from Google Cloud Console → Enable YouTube Data API v3 → Create Credentials → API Key

**Security Notes**:
- API keys stored in Azure Key Vault for production
- JWT secret key has fallback default (Program.cs:67-70) but should be overridden
- CORS allows all origins in development (Program.cs:16-24) - restrict for production
- Email service currently logs to console (EmailService.cs) - implement actual SMTP for production

## Development Workflow

### Adding New Features

1. **Database Changes**:
   - Add/modify entities in `Data/Entities/`
   - Update `AppDbContext.OnModelCreating()` with configuration
   - Create migration: `dotnet ef migrations add FeatureName`
   - Apply migration: `dotnet ef database update`

2. **Service Layer**:
   - Create interface in `Services/IServiceName.cs`
   - Implement service in `Services/ServiceName.cs`
   - Register in `Program.cs` dependency injection

3. **Controller**:
   - Add endpoints in appropriate controller or create new one
   - Add `[Authorize]` attribute if user-specific
   - Validate user ownership for protected resources

4. **Frontend** (if needed):
   - Update Alpine.js state/methods in `app.js`
   - Add UI elements in `index.html`
   - Style in `styles.css`

### Testing Strategy

- **Unit Tests**: Located in `YouTubeKurator.Tests/`
- **Framework**: xUnit with Moq for mocking
- **Database**: Uses `InMemory` provider for tests
- **Coverage**: Focus on service layer business logic and controller validation

### Common Pitfalls

1. **API Quota Exhaustion**: YouTube API has 10,000 units/day limit. Search costs ~100 units. Leverage caching!
2. **User Ownership**: Always validate `OwnerUserId` matches JWT claim in protected endpoints
3. **GUID Empty vs Null**: WatchLater uses `Guid.Empty` for global items, not null
4. **JSON Serialization**: Filters and DiscoveryProfile are stored as JSON strings - handle deserialization errors gracefully
5. **Service Worker Scope**: SW requires HTTPS or localhost to register
6. **Migration Conflicts**: Always pull latest before creating new migrations

### YouTube API Integration

**Search Flow** (YouTubeService.cs):
1. Search.List (snippet) → get video IDs
2. Videos.List (snippet, contentDetails, statistics) → get full video details
3. Parse ISO 8601 duration (XmlConvert)
4. Transform to internal `Video` DTO

**Quota Costs**:
- Search.List: ~100 units
- Videos.List: ~1 unit per video
- Total for 50 videos: ~150 units

**Error Handling**:
- Quota exceeded: Returns cached results with error metadata
- Network errors: Graceful fallback with user-friendly messages
- Invalid queries: Validates before making API calls

## Project Structure

```
youtube-kurator/
├── YouTubeKurator.Api/          # Main API project
│   ├── Controllers/                 # REST API endpoints
│   ├── Services/                    # Business logic
│   ├── Data/                        # EF Core context + entities
│   ├── Models/                      # DTOs and request models
│   ├── Migrations/                  # EF Core migrations
│   ├── wwwroot/                     # Frontend (Alpine.js PWA)
│   ├── Program.cs                   # App entry point + DI config
│   └── appsettings.json             # Configuration
├── YouTubeKurator.Tests/            # xUnit tests
├── spec/                            # Documentation
│   ├── v1/                          # Original spec + tasks
│   └── v2/                          # Multi-user + advanced features spec
├── Dockerfile                       # Docker configuration
├── youku.sln                        # Solution file
├── README.md                        # User documentation
└── DEPLOYMENT.md                    # Azure deployment guide
```

## Debugging Tips

1. **EF Core SQL Logging**: Set log level to `Debug` in appsettings to see generated SQL
2. **YouTube API Responses**: Check `ResultsJson` in `CachedSearches` table for raw API data
3. **JWT Token Issues**: Verify secret key length (must be ≥32 chars) and token expiration
4. **Frontend State**: Use Alpine.js DevTools browser extension
5. **Service Worker**: Check Application tab in Chrome DevTools for registration status

## Deployment

See `DEPLOYMENT.md` for full Azure Container Apps deployment guide.

**Quick checklist**:
- Configure Azure SQL connection string
- Set `YouTubeApi__ApiKey` from Key Vault
- Set `Jwt__SecretKey` to secure value
- Configure `ASPNETCORE_ENVIRONMENT=Production`
- Enable HTTPS (automatic in Container Apps)
- Restrict CORS origins
- Implement production email service
