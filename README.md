# YouTube Kurator - Personal YouTube Playlist Curator

A modern Progressive Web App (PWA) for creating and managing theme-based YouTube playlists with automatic video discovery and smart caching.

## Overview

YouTube Kurator allows you to create playlists based on search queries and automatically fetch the latest 50 videos from YouTube. With built-in caching and a clean, mobile-first interface, it's designed to help you discover and organize YouTube content efficiently.

## Features

- **Smart Playlist Management**: Create, edit, and delete playlists with custom search queries
- **Automatic Video Discovery**: Fetch up to 50 latest videos from YouTube per playlist
- **Intelligent Caching**: 1-hour cache to reduce API quota usage and improve performance
- **Progressive Web App**: Install on mobile or desktop for app-like experience
- **Responsive Design**: Works seamlessly on mobile, tablet, and desktop
- **Offline Support**: Service Worker caching for static assets
- **Real-time Updates**: Manual refresh to get the latest videos
- **Error Handling**: Graceful fallback when API quota is exceeded or network fails
- **Secure Configuration**: API keys stored in Azure Key Vault

## Tech Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| Backend | ASP.NET Core Web API | .NET 10.0 |
| Frontend | Vanilla JavaScript + Alpine.js | Alpine.js 3.x |
| Database (Dev) | SQLite | EF Core 10.0 |
| Database (Prod) | Azure SQL Database | - |
| ORM | Entity Framework Core | 10.0.1 |
| API Integration | YouTube Data API v3 | Google.Apis.YouTube.v3 |
| Containerization | Docker | - |
| Hosting | Azure Container Apps | - |
| Secrets Management | Azure Key Vault | - |

## Architecture

```
┌─────────────────────────────────────────┐
│      Azure Container Apps               │
│  ┌───────────────────────────────────┐  │
│  │   ASP.NET Core Web API            │  │
│  │  ┌─────────────┬───────────────┐  │  │
│  │  │  /wwwroot   │   /api        │  │  │
│  │  │  (Alpine.js │  (REST API)   │  │  │
│  │  │   PWA)      │               │  │  │
│  │  └─────────────┴───────────────┘  │  │
│  └───────────────────────────────────┘  │
└──────────────┬──────────────────────────┘
               │
    ┌──────────┴──────────┐
    ▼                     ▼
┌──────────┐      ┌──────────────┐
│YouTube   │      │Azure SQL     │
│API v3    │      │Database      │
│          │      │              │
│- Search  │      │- Playlists   │
│- Videos  │      │- CachedSearch│
└──────────┘      └──────────────┘
```

## Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- [Git](https://git-scm.com/downloads)
- YouTube Data API v3 key from [Google Cloud Console](https://console.cloud.google.com/)
- (Optional) [Docker](https://docs.docker.com/get-docker/) for containerized deployment

### Local Development Setup

#### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/youtube-kurator.git
cd youtube-kurator
```

#### 2. Configure YouTube API Key

Create or edit `YouTubeKurator.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=youtube-kurator.db"
  },
  "YouTubeApi": {
    "ApiKey": "YOUR_YOUTUBE_API_KEY_HERE"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**How to get a YouTube API Key**:
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing
3. Enable **YouTube Data API v3**
4. Go to **Credentials** > **Create Credentials** > **API Key**
5. Copy the key and paste in `appsettings.Development.json`

#### 3. Restore Dependencies

```bash
cd YouTubeKurator.Api
dotnet restore
```

#### 4. Create Database

```bash
dotnet ef database update
```

This creates a SQLite database file `youtube-kurator.db` with the required schema.

#### 5. Run the Application

```bash
dotnet run
```

The application will start on:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

#### 6. Open in Browser

Visit `http://localhost:5000` (or HTTPS URL if configured).

### Docker Setup

#### Build Docker Image

```bash
docker build -t youtube-kurator:latest .
```

#### Run Container

```bash
docker run -p 8080:80 \
  -e "YouTubeApi__ApiKey=YOUR_API_KEY" \
  -e "ConnectionStrings__DefaultConnection=Data Source=youtube-kurator.db" \
  youtube-kurator:latest
```

Visit `http://localhost:8080`.

## How to Use

### Creating a Playlist

1. Click **"+ Ny Spilleliste"** (New Playlist) button
2. Enter a **name** (e.g., "Best Music 2025")
3. Enter a **search query** (e.g., "best songs 2025")
4. Click **"Opprett"** (Create)

### Refreshing Videos

1. Click on a playlist from the list
2. Click **"Oppdater Videoer"** (Refresh Videos)
3. The app fetches 50 latest videos from YouTube
4. Videos are cached for 1 hour to save API quota

### Viewing Videos

- Scroll through the video list
- Click on any video thumbnail or title to open on YouTube
- Videos display:
  - Thumbnail
  - Title
  - Channel name
  - Duration
  - Published date
  - View count

### Editing a Playlist

1. Click on a playlist
2. Modify the name or search query fields
3. Changes are saved automatically

### Deleting a Playlist

1. Click on a playlist
2. Click **"Slett Spilleliste"** (Delete Playlist)
3. Confirm deletion

### Installing as PWA

**On Desktop (Chrome/Edge)**:
1. Click the install icon in the address bar
2. Click **"Install"**
3. App opens in standalone window

**On Mobile (Chrome/Safari)**:
1. Open app in browser
2. Tap **Share** button
3. Select **"Add to Home Screen"**
4. App icon appears on home screen

## API Documentation

### Base URL

- Local: `http://localhost:5000/api`
- Production: `https://your-app.azurecontainerapps.io/api`

### Endpoints

#### Playlists

##### Get All Playlists

```http
GET /api/playlists
```

**Response**:
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Music",
    "searchQuery": "best songs 2025",
    "createdUtc": "2025-12-17T10:00:00Z",
    "updatedUtc": "2025-12-17T10:00:00Z"
  }
]
```

##### Get Playlist by ID

```http
GET /api/playlists/{id}
```

**Response**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Music",
  "searchQuery": "best songs 2025",
  "createdUtc": "2025-12-17T10:00:00Z",
  "updatedUtc": "2025-12-17T10:00:00Z"
}
```

##### Create Playlist

```http
POST /api/playlists
Content-Type: application/json

{
  "name": "Tech Reviews",
  "searchQuery": "best tech reviews 2025"
}
```

**Response**: `201 Created` with playlist object

##### Update Playlist

```http
PUT /api/playlists/{id}
Content-Type: application/json

{
  "name": "Updated Name",
  "searchQuery": "updated search query"
}
```

**Response**: `200 OK` with updated playlist object

##### Delete Playlist

```http
DELETE /api/playlists/{id}
```

**Response**: `204 No Content`

#### Videos

##### Refresh Playlist Videos

```http
POST /api/playlists/{id}/refresh
```

**Response**:
```json
{
  "videos": [
    {
      "videoId": "dQw4w9WgXcQ",
      "title": "Rick Astley - Never Gonna Give You Up",
      "channelName": "Rick Astley",
      "thumbnailUrl": "https://i.ytimg.com/vi/dQw4w9WgXcQ/mqdefault.jpg",
      "duration": "00:03:33",
      "publishedAt": "2009-10-25T06:57:33Z",
      "viewCount": 1500000000
    }
  ],
  "fromCache": false,
  "cacheExpiresUtc": "2025-12-17T11:00:00Z",
  "error": null
}
```

**Error Response** (when quota exceeded):
```json
{
  "videos": [],
  "fromCache": true,
  "cacheExpiresUtc": "2025-12-17T10:30:00Z",
  "error": {
    "type": "QuotaExceeded",
    "message": "YouTube-kvoten er brukt opp for i dag. Prøv igjen i morgen."
  }
}
```

### Error Codes

| Error Type | HTTP Status | Description |
|-----------|-------------|-------------|
| `InvalidQuery` | 400 | Search query is empty or invalid |
| `QuotaExceeded` | 200* | YouTube API quota exhausted |
| `NetworkError` | 200* | Cannot connect to YouTube |
| `YouTubeApiError` | 200* | YouTube API returned an error |
| `GenericError` | 500 | Unexpected server error |

*Note: API returns 200 with cached data and error in response body for recoverable errors.

## Configuration

### Environment Variables

| Variable | Description | Required | Example |
|----------|-------------|----------|---------|
| `YouTubeApi__ApiKey` | YouTube Data API v3 key | Yes | `AIzaSyC...` |
| `ConnectionStrings__DefaultConnection` | Database connection string | Yes | `Data Source=youtube-kurator.db` |
| `ASPNETCORE_ENVIRONMENT` | Environment name | No | `Production` |
| `ASPNETCORE_URLS` | HTTP binding URLs | No | `http://+:80` |

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=youtube-kurator.db"
  },
  "YouTubeApi": {
    "ApiKey": ""
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

## Project Structure

```
youtube-kurator/
├── src/
│   └── YouTubeKurator.Api/           # Main ASP.NET Core project
│       ├── Controllers/               # API controllers
│       │   ├── PlaylistsController.cs # Playlist CRUD operations
│       │   └── PlaylistRequests.cs    # Request DTOs
│       ├── Services/                  # Business logic layer
│       │   ├── YouTubeService.cs      # YouTube API integration
│       │   ├── CacheService.cs        # Cache management
│       │   └── RefreshResponse.cs     # Response DTOs
│       ├── Data/                      # Database layer
│       │   ├── AppDbContext.cs        # EF Core DbContext
│       │   └── Entities/              # Database entities
│       │       ├── Playlist.cs        # Playlist model
│       │       ├── CachedSearch.cs    # Cache model
│       │       └── Video.cs           # Video DTO
│       ├── Migrations/                # EF Core migrations
│       │   └── 20251217202528_InitialCreate.cs
│       ├── wwwroot/                   # Frontend files
│       │   ├── index.html             # Main HTML page
│       │   ├── app.js                 # Alpine.js application
│       │   ├── styles.css             # CSS styling
│       │   ├── manifest.json          # PWA manifest
│       │   ├── sw.js                  # Service Worker
│       │   └── icon-*.png             # PWA icons
│       ├── Program.cs                 # Application entry point
│       ├── appsettings.json           # Configuration
│       └── YouTubeKurator.Api.csproj  # Project file
├── YouTubeKurator.Tests/             # Unit tests
│   ├── Controllers/
│   │   └── PlaylistsControllerTests.cs
│   ├── Services/
│   │   ├── YouTubeServiceTests.cs
│   │   └── CacheServiceTests.cs
│   └── YouTubeKurator.Tests.csproj
├── spec/                              # Documentation
│   ├── youtube-kurator-v1-spec.md     # Full specification
│   └── task-*.md                      # Implementation tasks
├── Dockerfile                         # Docker configuration
├── .dockerignore                      # Docker ignore file
├── .gitignore                         # Git ignore file
├── youku.sln                          # Visual Studio solution
├── README.md                          # This file
└── DEPLOYMENT.md                      # Deployment guide
```

## Testing

### Run Unit Tests

```bash
dotnet test YouTubeKurator.Tests/YouTubeKurator.Tests.csproj
```

### Run Tests with Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Manual Testing Checklist

See detailed checklist in [spec/task-10-2-manuell-testing.md](spec/task-10-2-manuell-testing.md)

**Quick Checklist**:
- [ ] Create playlist
- [ ] Edit playlist name and search query
- [ ] Refresh videos (first time - from YouTube)
- [ ] Refresh videos (second time - from cache)
- [ ] Click video to open on YouTube
- [ ] Delete playlist
- [ ] Test on mobile device
- [ ] Install as PWA
- [ ] Test offline behavior

## Deployment

### Deploy to Azure Container Apps

See comprehensive deployment guide: [DEPLOYMENT.md](DEPLOYMENT.md)

**Quick Deploy Steps**:

1. **Setup Azure Resources**:
   ```bash
   az group create --name youtube-kurator-rg --location norwayeast
   az sql server create --name youtube-kurator-server --resource-group youtube-kurator-rg ...
   az sql db create --server youtube-kurator-server --name youtube-kurator-db ...
   az keyvault create --name youtube-kurator-kv --resource-group youtube-kurator-rg ...
   az acr create --name youtubekuratoracr --resource-group youtube-kurator-rg ...
   az containerapp env create --name youtube-kurator-env --resource-group youtube-kurator-rg ...
   ```

2. **Build & Push Docker Image**:
   ```bash
   docker build -t youtubekuratoracr.azurecr.io/youtube-kurator:latest .
   docker push youtubekuratoracr.azurecr.io/youtube-kurator:latest
   ```

3. **Deploy Container App**:
   ```bash
   az containerapp create --name youtube-kurator --resource-group youtube-kurator-rg ...
   ```

Full instructions in [DEPLOYMENT.md](DEPLOYMENT.md).

## Troubleshooting

### "YOUTUBE_API_KEY er ikke konfigurert"

**Cause**: API key not configured in `appsettings.Development.json`

**Solution**: Add your YouTube API key:
```json
{
  "YouTubeApi": {
    "ApiKey": "YOUR_API_KEY_HERE"
  }
}
```

### "Database file not found"

**Cause**: Database not created or migrations not run

**Solution**: Run migrations:
```bash
cd YouTubeKurator.Api
dotnet ef database update
```

### "YouTube quota exceeded"

**Cause**: Daily API quota limit reached (10,000 units/day)

**Solution**:
- Wait until quota resets (midnight Pacific Time)
- Use cached results (automatically served when quota exceeded)
- Request quota increase in Google Cloud Console
- Upgrade to paid plan for higher quota

### Service Worker not registering

**Cause**: Service Workers require HTTPS or localhost

**Solution**:
- Use `https://localhost:5001` instead of HTTP
- Or deploy to Azure (automatic HTTPS)
- Or use `localhost` (not `127.0.0.1`)

### Videos not displaying thumbnails

**Cause**: CORS or network issues

**Solution**:
- Check browser console for errors
- Verify YouTube API response includes thumbnail URLs
- Check network tab for failed image requests

### Container won't start in Azure

**Cause**: Missing environment variables or database connection issues

**Solution**:
1. Check Container App logs:
   ```bash
   az containerapp logs show --name youtube-kurator --resource-group youtube-kurator-rg
   ```
2. Verify environment variables are set
3. Check Key Vault permissions
4. Verify SQL firewall allows Azure services

## Performance

### Caching Strategy

- **Cache Duration**: 1 hour per search query
- **Cache Key**: Search query string (case-sensitive)
- **Cache Storage**: Azure SQL Database (CachedSearches table)
- **Cache Invalidation**: Automatic after 1 hour
- **Benefits**:
  - Reduces YouTube API quota usage
  - Faster response times for repeated queries
  - Graceful fallback when API quota exceeded

### API Quota Management

**YouTube Data API v3 Quotas**:
- **Daily Limit**: 10,000 units/day (free tier)
- **Search Cost**: ~100 units per search
- **Videos.list Cost**: ~1 unit per video
- **Estimated Searches/Day**: ~100 searches (with video details)

**Optimization Tips**:
- Leverage 1-hour cache
- Avoid refreshing same playlist multiple times
- Request quota increase for high-traffic apps
- Monitor usage in Google Cloud Console

## Security

### Best Practices

1. **Never commit API keys** to version control
2. **Use environment variables** for sensitive configuration
3. **Use Azure Key Vault** in production
4. **Enable HTTPS** for all production traffic
5. **Implement rate limiting** if opening to public
6. **Validate user input** to prevent injection attacks
7. **Keep dependencies updated** for security patches

### CORS Configuration

The API allows all origins in development:
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

**For production**, restrict to specific origins:
```csharp
policy.WithOrigins("https://yourdomain.com")
      .AllowAnyMethod()
      .AllowAnyHeader();
```

## Contributing

Contributions are welcome! Please follow these guidelines:

### How to Contribute

1. **Fork** the repository
2. **Create a branch**: `git checkout -b feature/your-feature-name`
3. **Make changes** and test thoroughly
4. **Write tests** for new functionality
5. **Commit**: `git commit -m "Add feature: description"`
6. **Push**: `git push origin feature/your-feature-name`
7. **Create Pull Request** with detailed description

### Coding Standards

- **C#**: Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- **JavaScript**: ES6+, 2-space indentation, semicolons
- **CSS**: BEM naming convention where applicable
- **Comments**: Add XML docs for public APIs

### Testing Requirements

- All new features must include unit tests
- Maintain or improve code coverage
- Run `dotnet test` before submitting PR
- Include integration tests for API endpoints

### Commit Messages

- Use imperative mood ("Add feature" not "Added feature")
- Keep first line under 50 characters
- Add detailed description if needed
- Reference issue numbers: "Fix #123"

## Roadmap

### Version 1.0 (Current)
- [x] Basic playlist CRUD operations
- [x] YouTube API integration
- [x] Smart caching (1 hour)
- [x] Responsive PWA
- [x] Azure deployment support

### Version 1.1 (Planned)
- [ ] Multiple filter options (duration, language, channel)
- [ ] Video marking (watched/unwatched)
- [ ] Sorting options (views, date, relevance)
- [ ] Export playlists to YouTube
- [ ] Dark mode support

### Version 2.0 (Future)
- [ ] User authentication (magic link)
- [ ] Multi-user support
- [ ] Cross-device sync
- [ ] Watch later functionality
- [ ] Discovery mode with recommendations
- [ ] Duplicate video detection

## License

This project is licensed under the MIT License - see below for details:

```
MIT License

Copyright (c) 2025 YouTube Kurator

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## Acknowledgments

- **YouTube Data API v3** by Google
- **Alpine.js** by Caleb Porzio
- **ASP.NET Core** by Microsoft
- **Entity Framework Core** by Microsoft
- **Azure Container Apps** by Microsoft

## Support

### Issues & Bugs

Found a bug? Please [create an issue](https://github.com/yourusername/youtube-kurator/issues) with:
- Description of the bug
- Steps to reproduce
- Expected behavior
- Actual behavior
- Screenshots (if applicable)
- Environment details (OS, browser, .NET version)

### Feature Requests

Have an idea? [Create a feature request](https://github.com/yourusername/youtube-kurator/issues) with:
- Description of the feature
- Use case / problem it solves
- Proposed implementation (optional)

### Questions

For questions and discussions:
- Check existing [issues](https://github.com/yourusername/youtube-kurator/issues)
- Start a [discussion](https://github.com/yourusername/youtube-kurator/discussions)
- Read the [documentation](spec/youtube-kurator-v1-spec.md)

## Contact

- **Project Repository**: [GitHub](https://github.com/yourusername/youtube-kurator)
- **Documentation**: [spec/youtube-kurator-v1-spec.md](spec/youtube-kurator-v1-spec.md)
- **Deployment Guide**: [DEPLOYMENT.md](DEPLOYMENT.md)

---

**Version**: 1.0.0
**Last Updated**: 2025-12-18
**Status**: Production Ready
**Maintained By**: YouTube Kurator Team

Made with passion and .NET
