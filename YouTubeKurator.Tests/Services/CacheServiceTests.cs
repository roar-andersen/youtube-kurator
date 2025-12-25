using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using YouTubeKurator.Api.Data;
using YouTubeKurator.Api.Services;
using YouTubeKurator.Api.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

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
        public async Task GetOrFetchVideosAsync_WithEmptyQuery_ReturnsInvalidQueryError()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var youtubeServiceMock = new Mock<YouTubeService>(MockBehavior.Strict, (object)null);
            var cacheService = new CacheService(context, youtubeServiceMock.Object);

            // Act
            var (videos, fromCache, cacheExpiresUtc, errorType, errorMessage) =
                await cacheService.GetOrFetchVideosAsync("");

            // Assert
            Assert.Equal("InvalidQuery", errorType);
            Assert.Equal("Søkeordet er tomt.", errorMessage);
            Assert.Empty(videos);
            Assert.False(fromCache);
            Assert.Null(cacheExpiresUtc);
        }

        [Fact]
        public async Task GetOrFetchVideosAsync_WithValidCache_ReturnsCachedData()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var testVideos = new List<Video>
            {
                new Video
                {
                    VideoId = "test123",
                    Title = "Test Video",
                    ChannelName = "Test Channel",
                    ThumbnailUrl = "http://test.com/thumb.jpg",
                    Duration = TimeSpan.FromMinutes(5),
                    PublishedAt = DateTime.UtcNow.AddDays(-1),
                    ViewCount = 1000
                }
            };
            var expiresUtc = DateTime.UtcNow.AddHours(1);
            var cachedSearch = new CachedSearch
            {
                Id = Guid.NewGuid(),
                SearchQuery = "test query",
                ResultsJson = JsonSerializer.Serialize(testVideos),
                FetchedUtc = DateTime.UtcNow.AddMinutes(-30),
                ExpiresUtc = expiresUtc
            };
            context.CachedSearches.Add(cachedSearch);
            await context.SaveChangesAsync();

            var youtubeServiceMock = new Mock<YouTubeService>(MockBehavior.Strict, (object)null);
            var cacheService = new CacheService(context, youtubeServiceMock.Object);

            // Act
            var (videos, fromCache, cacheExpiresUtcResult, errorType, errorMessage) =
                await cacheService.GetOrFetchVideosAsync("test query");

            // Assert
            Assert.Single(videos);
            Assert.Equal("test123", videos[0].VideoId);
            Assert.Equal("Test Video", videos[0].Title);
            Assert.True(fromCache);
            Assert.Equal(expiresUtc, cacheExpiresUtcResult);
            Assert.Null(errorType);
            Assert.Null(errorMessage);
        }

        [Fact]
        public async Task GetOrFetchVideosAsync_WithExpiredCache_FetchesFromYouTube()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var expiredCache = new CachedSearch
            {
                Id = Guid.NewGuid(),
                SearchQuery = "test query",
                ResultsJson = JsonSerializer.Serialize(new List<Video>()),
                FetchedUtc = DateTime.UtcNow.AddHours(-2),
                ExpiresUtc = DateTime.UtcNow.AddMinutes(-30) // Expired
            };
            context.CachedSearches.Add(expiredCache);
            await context.SaveChangesAsync();

            var newVideos = new List<Video>
            {
                new Video
                {
                    VideoId = "new123",
                    Title = "New Video",
                    ChannelName = "New Channel",
                    ThumbnailUrl = "http://test.com/new.jpg",
                    Duration = TimeSpan.FromMinutes(3),
                    PublishedAt = DateTime.UtcNow,
                    ViewCount = 500
                }
            };

            var youtubeServiceMock = new Mock<YouTubeService>(MockBehavior.Strict, (object)null);
            youtubeServiceMock.Setup(x => x.SearchVideosAsync("test query", 50))
                .ReturnsAsync((newVideos, null, null));

            var cacheService = new CacheService(context, youtubeServiceMock.Object);

            // Act
            var (videos, fromCache, cacheExpiresUtc, errorType, errorMessage) =
                await cacheService.GetOrFetchVideosAsync("test query");

            // Assert
            Assert.Single(videos);
            Assert.Equal("new123", videos[0].VideoId);
            Assert.False(fromCache);
            Assert.NotNull(cacheExpiresUtc);
            Assert.Null(errorType);
            Assert.Null(errorMessage);
            youtubeServiceMock.Verify(x => x.SearchVideosAsync("test query", 50), Times.Once);
        }

        [Fact]
        public async Task GetOrFetchVideosAsync_WithNoCache_FetchesFromYouTube()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var newVideos = new List<Video>
            {
                new Video
                {
                    VideoId = "fresh123",
                    Title = "Fresh Video",
                    ChannelName = "Fresh Channel",
                    ThumbnailUrl = "http://test.com/fresh.jpg",
                    Duration = TimeSpan.FromMinutes(10),
                    PublishedAt = DateTime.UtcNow,
                    ViewCount = 2000
                }
            };

            var youtubeServiceMock = new Mock<YouTubeService>(MockBehavior.Strict, (object)null);
            youtubeServiceMock.Setup(x => x.SearchVideosAsync("fresh query", 50))
                .ReturnsAsync((newVideos, null, null));

            var cacheService = new CacheService(context, youtubeServiceMock.Object);

            // Act
            var (videos, fromCache, cacheExpiresUtc, errorType, errorMessage) =
                await cacheService.GetOrFetchVideosAsync("fresh query");

            // Assert
            Assert.Single(videos);
            Assert.Equal("fresh123", videos[0].VideoId);
            Assert.False(fromCache);
            Assert.NotNull(cacheExpiresUtc);
            Assert.Null(errorType);
            Assert.Null(errorMessage);

            // Verify cache was saved
            var savedCache = await context.CachedSearches.FirstOrDefaultAsync(cs => cs.SearchQuery == "fresh query");
            Assert.NotNull(savedCache);
            Assert.Equal("fresh query", savedCache.SearchQuery);
        }

        [Fact]
        public async Task GetOrFetchVideosAsync_WithQuotaError_ReturnsCachedDataWithError()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var oldVideos = new List<Video>
            {
                new Video
                {
                    VideoId = "old123",
                    Title = "Old Video",
                    ChannelName = "Old Channel",
                    ThumbnailUrl = "http://test.com/old.jpg",
                    Duration = TimeSpan.FromMinutes(5),
                    PublishedAt = DateTime.UtcNow.AddDays(-2),
                    ViewCount = 100
                }
            };
            var expiredCache = new CachedSearch
            {
                Id = Guid.NewGuid(),
                SearchQuery = "test query",
                ResultsJson = JsonSerializer.Serialize(oldVideos),
                FetchedUtc = DateTime.UtcNow.AddHours(-2),
                ExpiresUtc = DateTime.UtcNow.AddMinutes(-30) // Expired
            };
            context.CachedSearches.Add(expiredCache);
            await context.SaveChangesAsync();

            var youtubeServiceMock = new Mock<YouTubeService>(MockBehavior.Strict, (object)null);
            youtubeServiceMock.Setup(x => x.SearchVideosAsync("test query", 50))
                .ReturnsAsync((new List<Video>(), "QuotaExceeded", "YouTube-kvoten er brukt opp for i dag. Prøv igjen i morgen."));

            var cacheService = new CacheService(context, youtubeServiceMock.Object);

            // Act
            var (videos, fromCache, cacheExpiresUtc, errorType, errorMessage) =
                await cacheService.GetOrFetchVideosAsync("test query");

            // Assert
            Assert.Single(videos);
            Assert.Equal("old123", videos[0].VideoId);
            Assert.True(fromCache);
            Assert.Equal("QuotaExceeded", errorType);
            Assert.Equal("YouTube-kvoten er brukt opp for i dag. Prøv igjen i morgen.", errorMessage);
        }

        [Fact]
        public async Task InvalidateCacheAsync_RemovesCache()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var cachedSearch = new CachedSearch
            {
                Id = Guid.NewGuid(),
                SearchQuery = "test query",
                ResultsJson = JsonSerializer.Serialize(new List<Video>()),
                FetchedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.AddHours(1)
            };
            context.CachedSearches.Add(cachedSearch);
            await context.SaveChangesAsync();

            var youtubeServiceMock = new Mock<YouTubeService>(MockBehavior.Strict, (object)null);
            var cacheService = new CacheService(context, youtubeServiceMock.Object);

            // Act
            await cacheService.InvalidateCacheAsync("test query");

            // Assert
            var cached = await context.CachedSearches.FirstOrDefaultAsync(cs => cs.SearchQuery == "test query");
            Assert.Null(cached);
        }

        [Fact]
        public async Task InvalidateCacheAsync_WithNonExistentQuery_DoesNotThrow()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var youtubeServiceMock = new Mock<YouTubeService>(MockBehavior.Strict, (object)null);
            var cacheService = new CacheService(context, youtubeServiceMock.Object);

            // Act & Assert (should not throw)
            await cacheService.InvalidateCacheAsync("non-existent query");
        }

        [Fact]
        public async Task CleanupExpiredCacheAsync_RemovesOnlyExpiredEntries()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var expiredCache1 = new CachedSearch
            {
                Id = Guid.NewGuid(),
                SearchQuery = "expired1",
                ResultsJson = JsonSerializer.Serialize(new List<Video>()),
                FetchedUtc = DateTime.UtcNow.AddHours(-2),
                ExpiresUtc = DateTime.UtcNow.AddMinutes(-30)
            };
            var expiredCache2 = new CachedSearch
            {
                Id = Guid.NewGuid(),
                SearchQuery = "expired2",
                ResultsJson = JsonSerializer.Serialize(new List<Video>()),
                FetchedUtc = DateTime.UtcNow.AddHours(-2),
                ExpiresUtc = DateTime.UtcNow.AddMinutes(-10)
            };
            var validCache = new CachedSearch
            {
                Id = Guid.NewGuid(),
                SearchQuery = "valid",
                ResultsJson = JsonSerializer.Serialize(new List<Video>()),
                FetchedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.AddHours(1)
            };
            context.CachedSearches.AddRange(expiredCache1, expiredCache2, validCache);
            await context.SaveChangesAsync();

            var youtubeServiceMock = new Mock<YouTubeService>(MockBehavior.Strict, (object)null);
            var cacheService = new CacheService(context, youtubeServiceMock.Object);

            // Act
            await cacheService.CleanupExpiredCacheAsync();

            // Assert
            var remainingCache = await context.CachedSearches.ToListAsync();
            Assert.Single(remainingCache);
            Assert.Equal("valid", remainingCache[0].SearchQuery);
        }
    }
}
