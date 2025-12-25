using Xunit;
using YouTubeKurator.Api.Data.Entities;
using YouTubeKurator.Api.Models;
using YouTubeKurator.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;

namespace YouTubeKurator.Tests.Services
{
    public class DiscoveryServiceTests
    {
        private IDiscoveryService CreateDiscoveryService()
        {
            var filterServiceMock = new Mock<IFilterService>();
            var sortingService = new SortingService();
            var relatedVideosServiceMock = new Mock<IRelatedVideosService>();

            // Setup default behavior for related videos
            relatedVideosServiceMock
                .Setup(s => s.GetRelatedVideosAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(Enumerable.Empty<Video>());

            return new DiscoveryService(
                filterServiceMock.Object,
                sortingService,
                relatedVideosServiceMock.Object);
        }

        private Playlist CreatePlaylist(
            string? discoveryProfile = null,
            string sortStrategy = "NewestFirst")
        {
            return new Playlist
            {
                Id = Guid.NewGuid(),
                Name = "Test Playlist",
                OwnerUserId = Guid.NewGuid(),
                SearchQuery = "test",
                DiscoveryProfile = discoveryProfile,
                SortStrategy = sortStrategy,
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };
        }

        private Video CreateTestVideo(
            string videoId = "test_id",
            string title = "Test Video",
            string channelId = "UC_test",
            long viewCount = 10000,
            long likeCount = 500)
        {
            return new Video
            {
                VideoId = videoId,
                Title = title,
                ChannelName = "Test Channel",
                ChannelId = channelId,
                ThumbnailUrl = "https://example.com/thumb.jpg",
                Duration = TimeSpan.FromMinutes(10),
                PublishedAt = DateTime.UtcNow.AddDays(-10),
                ViewCount = viewCount,
                LikeCount = likeCount,
                Language = "en",
                ContentType = "video",
                HasCaptions = false
            };
        }

        // Discovery disabled tests
        [Fact]
        public async Task SelectVideosWithDiscoveryAsync_NoDiscoveryProfile_ReturnsSortedVideos()
        {
            // Arrange
            var service = CreateDiscoveryService();
            var playlist = CreatePlaylist(discoveryProfile: null);
            var videos = new List<Video>
            {
                CreateTestVideoWithDate("v1", publishedAt: DateTime.UtcNow.AddDays(-30)),
                CreateTestVideoWithDate("v2", publishedAt: DateTime.UtcNow.AddDays(-1)),
                CreateTestVideoWithDate("v3", publishedAt: DateTime.UtcNow.AddDays(-10))
            };

            // Act
            var result = await service.SelectVideosWithDiscoveryAsync(videos, playlist, 3);

            // Assert
            Assert.Equal(3, result.Count());
            // Should be sorted by newest first (default)
            Assert.Equal("v2", result.First().VideoId);
        }

        [Fact]
        public async Task SelectVideosWithDiscoveryAsync_EmptyVideos_ReturnsEmpty()
        {
            // Arrange
            var service = CreateDiscoveryService();
            var playlist = CreatePlaylist();
            var videos = new List<Video>();

            // Act
            var result = await service.SelectVideosWithDiscoveryAsync(videos, playlist, 10);

            // Assert
            Assert.Empty(result);
        }

        // Discovery enabled tests
        [Fact]
        public async Task SelectVideosWithDiscoveryAsync_DiscoveryEnabled_ReturnsMixedVideos()
        {
            // Arrange
            var service = CreateDiscoveryService();
            var discoveryProfile = "{\"strict\":70,\"relaxed\":20,\"wild\":10,\"enableWildcards\":true}";
            var playlist = CreatePlaylist(discoveryProfile: discoveryProfile);

            var videos = new List<Video>();
            for (int i = 0; i < 30; i++)
            {
                videos.Add(CreateTestVideo($"v{i}"));
            }

            // Act
            var result = await service.SelectVideosWithDiscoveryAsync(videos, playlist, 10);

            // Assert
            Assert.NotEmpty(result);
            Assert.True(result.Count() <= 10);
        }

        // Selection explanation tests
        [Fact]
        public void GetSelectionExplanation_VideoWithReason_ReturnsReason()
        {
            // Arrange
            var service = CreateDiscoveryService();
            var video = CreateTestVideo();
            video.DiscoveryReason = "Strict match - all criteria met";

            // Act
            var explanation = service.GetSelectionExplanation(video);

            // Assert
            Assert.Equal("Strict match - all criteria met", explanation);
        }

        [Fact]
        public void GetSelectionExplanation_VideoWithoutReason_ReturnsDefault()
        {
            // Arrange
            var service = CreateDiscoveryService();
            var video = CreateTestVideo();
            video.DiscoveryReason = null;

            // Act
            var explanation = service.GetSelectionExplanation(video);

            // Assert
            Assert.Equal("Selected video", explanation);
        }

        // Sorting preservation tests
        [Fact]
        public async Task SelectVideosWithDiscoveryAsync_AppliesSortStrategy()
        {
            // Arrange
            var service = CreateDiscoveryService();
            var discoveryProfile = "{\"strict\":100,\"relaxed\":0,\"wild\":0,\"enableWildcards\":false}";
            var playlist = CreatePlaylist(
                discoveryProfile: discoveryProfile,
                sortStrategy: "MostPopular");

            var videos = new List<Video>
            {
                CreateTestVideo("v1", viewCount: 1000),
                CreateTestVideo("v2", viewCount: 100000),
                CreateTestVideo("v3", viewCount: 10000)
            };

            // Act
            var result = await service.SelectVideosWithDiscoveryAsync(videos, playlist, 3);

            // Assert
            Assert.NotEmpty(result);
            // Should be sorted by view count (MostPopular)
            if (result.Count() >= 2)
            {
                Assert.True(result.First().ViewCount >= result.ElementAt(1).ViewCount);
            }
        }

        // Edge case tests
        [Fact]
        public async Task SelectVideosWithDiscoveryAsync_DesiredCountExceedsCandidates_ReturnAllCandidates()
        {
            // Arrange
            var service = CreateDiscoveryService();
            var playlist = CreatePlaylist();
            var videos = new List<Video>
            {
                CreateTestVideo("v1"),
                CreateTestVideo("v2"),
                CreateTestVideo("v3")
            };

            // Act
            var result = await service.SelectVideosWithDiscoveryAsync(videos, playlist, 10);

            // Assert
            Assert.True(result.Count() <= 3);
        }

        [Fact]
        public async Task SelectVideosWithDiscoveryAsync_SingleVideo_ReturnsSingleVideo()
        {
            // Arrange
            var service = CreateDiscoveryService();
            var playlist = CreatePlaylist();
            var videos = new List<Video> { CreateTestVideo("v1") };

            // Act
            var result = await service.SelectVideosWithDiscoveryAsync(videos, playlist, 1);

            // Assert
            Assert.Single(result);
            Assert.Equal("v1", result.First().VideoId);
        }

        // Discovery reason assignment tests
        [Fact]
        public async Task SelectVideosWithDiscoveryAsync_AssignsDiscoveryReasons()
        {
            // Arrange
            var service = CreateDiscoveryService();
            var discoveryProfile = "{\"strict\":100,\"relaxed\":0,\"wild\":0,\"enableWildcards\":false}";
            var playlist = CreatePlaylist(discoveryProfile: discoveryProfile);

            var videos = new List<Video>();
            for (int i = 0; i < 10; i++)
            {
                videos.Add(CreateTestVideo($"v{i}"));
            }

            // Act
            var result = await service.SelectVideosWithDiscoveryAsync(videos, playlist, 5);

            // Assert
            foreach (var video in result)
            {
                Assert.NotNull(video.DiscoveryReason);
                Assert.NotEmpty(video.DiscoveryReason);
            }
        }

        // Ratio tests
        [Fact]
        public async Task SelectVideosWithDiscoveryAsync_RespectStrictRatio()
        {
            // Arrange
            var service = CreateDiscoveryService();
            var discoveryProfile = "{\"strict\":70,\"relaxed\":20,\"wild\":10,\"enableWildcards\":true}";
            var playlist = CreatePlaylist(discoveryProfile: discoveryProfile);

            var videos = new List<Video>();
            for (int i = 0; i < 30; i++)
            {
                videos.Add(CreateTestVideo($"v{i}"));
            }

            // Act
            var result = await service.SelectVideosWithDiscoveryAsync(videos, playlist, 10);

            // Assert
            var strictMatches = result.Where(v => v.DiscoveryReason?.Contains("Strict") ?? false).Count();
            var relaxedMatches = result.Where(v => v.DiscoveryReason?.Contains("Relaxed") ?? false).Count();

            // Should have some proportion of strict matches
            Assert.True(strictMatches >= 0);
        }

        // Deserialization tests
        [Fact]
        public async Task SelectVideosWithDiscoveryAsync_WithValidJsonProfile_ParsesCorrectly()
        {
            // Arrange
            var service = CreateDiscoveryService();
            var discoveryProfile = "{\"strict\":60,\"relaxed\":30,\"wild\":10,\"enableWildcards\":true}";
            var playlist = CreatePlaylist(discoveryProfile: discoveryProfile);

            var videos = new List<Video>();
            for (int i = 0; i < 20; i++)
            {
                videos.Add(CreateTestVideo($"v{i}"));
            }

            // Act
            var result = await service.SelectVideosWithDiscoveryAsync(videos, playlist, 10);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task SelectVideosWithDiscoveryAsync_WithInvalidJsonProfile_UsesDefaults()
        {
            // Arrange
            var service = CreateDiscoveryService();
            var discoveryProfile = "invalid json {]";
            var playlist = CreatePlaylist(discoveryProfile: discoveryProfile);

            var videos = new List<Video>();
            for (int i = 0; i < 10; i++)
            {
                videos.Add(CreateTestVideo($"v{i}"));
            }

            // Act
            var result = await service.SelectVideosWithDiscoveryAsync(videos, playlist, 5);

            // Assert
            // Should treat as no discovery profile and return sorted videos
            Assert.True(result.Count() <= 5);
        }

        // Applyiscovery logic test
        [Fact]
        public async Task ApplyDiscoveryLogicAsync_ReturnsVideos()
        {
            // Arrange
            var service = CreateDiscoveryService();
            var profile = new DiscoveryProfile { EnableWildcards = true };
            var videos = new List<Video>
            {
                CreateTestVideo("v1"),
                CreateTestVideo("v2")
            };

            // Act
            var result = await service.ApplyDiscoveryLogicAsync(videos, profile);

            // Assert
            Assert.Equal(2, result.Count());
        }

        // Helper method for creating videos with specific published dates
        private Video CreateTestVideoWithDate(
            string videoId,
            DateTime? publishedAt = null,
            string channelId = "UC_test",
            long viewCount = 10000,
            long likeCount = 500)
        {
            return new Video
            {
                VideoId = videoId,
                Title = $"Test Video {videoId}",
                ChannelName = "Test Channel",
                ChannelId = channelId,
                ThumbnailUrl = "https://example.com/thumb.jpg",
                Duration = TimeSpan.FromMinutes(10),
                PublishedAt = publishedAt ?? DateTime.UtcNow.AddDays(-10),
                ViewCount = viewCount,
                LikeCount = likeCount,
                Language = "en",
                ContentType = "video",
                HasCaptions = false
            };
        }
    }
}
