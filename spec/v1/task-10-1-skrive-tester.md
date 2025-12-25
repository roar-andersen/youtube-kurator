# Oppgave 10.1: Skrive enhetstester for backend-services

## Fase
Fase 10: Testing og validering

## Avhengigheter
- Oppgave 4.1 (YouTubeService)
- Oppgave 4.2 (CacheService)
- Oppgave 3.1 (PlaylistsController)

## Formål
Implementere enhetstester for backend-logikk for å sikre pålitelighet.

## Oppgavebeskrivelse

### 1. Opprett test-prosjekt
```bash
cd YouTubeKurator
dotnet new xunit -n YouTubeKurator.Tests
cd YouTubeKurator.Tests
dotnet add reference ../src/YouTubeKurator.Api/YouTubeKurator.Api.csproj
```

### 2. Installer test-avhengigheter
```bash
dotnet add package Moq
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

### 3. Lag YouTubeServiceTests
Opprett `YouTubeKurator.Tests/Services/YouTubeServiceTests.cs`:

```csharp
using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using YouTubeKurator.Api.Services;

namespace YouTubeKurator.Tests.Services
{
    public class YouTubeServiceTests
    {
        [Fact]
        public async Task SearchVideosAsync_WithValidQuery_ReturnsVideos()
        {
            // Arrange
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["YouTubeApi:ApiKey"]).Returns("test-key");
            var service = new YouTubeService(configMock.Object);

            // Act
            var (videos, errorType, errorMessage) = await service.SearchVideosAsync("test query");

            // Assert
            Assert.NotNull(videos);
            // Note: This test may fail without real API key - mock YouTube API for proper testing
        }

        [Fact]
        public async Task SearchVideosAsync_WithEmptyQuery_ReturnsError()
        {
            // Arrange
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["YouTubeApi:ApiKey"]).Returns("test-key");
            var service = new YouTubeService(configMock.Object);

            // Act
            var (videos, errorType, errorMessage) = await service.SearchVideosAsync("");

            // Assert
            Assert.Equal("InvalidQuery", errorType);
            Assert.Empty(videos);
        }
    }
}
```

### 4. Lag CacheServiceTests
Opprett `YouTubeKurator.Tests/Services/CacheServiceTests.cs`:

```csharp
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using YouTubeKurator.Api.Data;
using YouTubeKurator.Api.Services;

namespace YouTubeKurator.Tests.Services
{
    public class CacheServiceTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetOrFetchVideosAsync_WithValidCache_ReturnsCachedData()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var youtubeServiceMock = new Mock<YouTubeService>(null);
            var cacheService = new CacheService(context, youtubeServiceMock.Object);

            // Act & Assert
            // TODO: Implement proper test with cached data
        }

        [Fact]
        public async Task InvalidateCacheAsync_RemovesCache()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var youtubeServiceMock = new Mock<YouTubeService>(null);
            var cacheService = new CacheService(context, youtubeServiceMock.Object);

            // Act
            await cacheService.InvalidateCacheAsync("test query");

            // Assert
            var cached = await context.CachedSearches.FirstOrDefaultAsync(cs => cs.SearchQuery == "test query");
            Assert.Null(cached);
        }
    }
}
```

### 5. Lag PlaylistsControllerTests
Opprett `YouTubeKurator.Tests/Controllers/PlaylistsControllerTests.cs`:

```csharp
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using YouTubeKurator.Api.Controllers;
using YouTubeKurator.Api.Data;
using YouTubeKurator.Api.Data.Entities;

namespace YouTubeKurator.Tests.Controllers
{
    public class PlaylistsControllerTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetPlaylists_ReturnsAllPlaylists()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var playlist = new Playlist
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                SearchQuery = "test",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };
            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            var controller = new PlaylistsController(context, null);

            // Act
            var result = await controller.GetPlaylists();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreatePlaylist_WithValidData_CreatesPlaylist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var controller = new PlaylistsController(context, null);
            var request = new CreatePlaylistRequest { Name = "Test", SearchQuery = "test" };

            // Act
            var result = await controller.CreatePlaylist(request);

            // Assert
            var playlists = await context.Playlists.ToListAsync();
            Assert.NotEmpty(playlists);
        }
    }
}
```

### 6. Kjør tester
```bash
dotnet test
```

## Akseptansekriterier
- [ ] Test-prosjekt opprettet
- [ ] YouTubeServiceTests eksisterer
- [ ] CacheServiceTests eksisterer
- [ ] PlaylistsControllerTests eksisterer
- [ ] `dotnet test` kjører uten feil
- [ ] Minst 5 tests implementert
- [ ] Tests har Arrange-Act-Assert-struktur

## Referanser
- [xUnit Documentation](https://xunit.net/)
- [Moq Library](https://github.com/moq/moq4)
- [Entity Framework Testing](https://learn.microsoft.com/en-us/ef/core/testing/)
