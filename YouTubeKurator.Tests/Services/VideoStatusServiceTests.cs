using Xunit;
using Microsoft.EntityFrameworkCore;
using YouTubeKurator.Api.Data;
using YouTubeKurator.Api.Data.Entities;
using YouTubeKurator.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YouTubeKurator.Tests.Services
{
    public class VideoStatusServiceTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private async Task<(Guid PlaylistId, Guid UserId)> SetupPlaylistAsync(AppDbContext context)
        {
            var userId = Guid.NewGuid();
            var playlistId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = $"user{userId}@test.com",
                CreatedUtc = DateTime.UtcNow,
                IsActive = true
            };

            var playlist = new Playlist
            {
                Id = playlistId,
                Name = "Test Playlist",
                OwnerUserId = userId,
                SearchQuery = "test",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            context.Users.Add(user);
            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            return (playlistId, userId);
        }

        // GetStatusAsync Tests
        [Fact]
        public async Task GetStatusAsync_VideoStatusExists_ReturnsStatus()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var (playlistId, _) = await SetupPlaylistAsync(context);
            var videoId = "video_123";
            var status = new VideoStatus
            {
                PlaylistId = playlistId,
                VideoId = videoId,
                Status = VideoStatusEnum.Seen,
                FirstSeenUtc = DateTime.UtcNow,
                LastUpdatedUtc = DateTime.UtcNow
            };
            context.VideoStatuses.Add(status);
            await context.SaveChangesAsync();

            var service = new VideoStatusService(context);

            // Act
            var result = await service.GetStatusAsync(playlistId, videoId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(videoId, result.VideoId);
            Assert.Equal(VideoStatusEnum.Seen, result.Status);
        }

        [Fact]
        public async Task GetStatusAsync_VideoStatusDoesNotExist_ReturnsNull()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var (playlistId, _) = await SetupPlaylistAsync(context);
            var service = new VideoStatusService(context);

            // Act
            var result = await service.GetStatusAsync(playlistId, "nonexistent_video");

            // Assert
            Assert.Null(result);
        }

        // UpdateStatusAsync Tests
        [Fact]
        public async Task UpdateStatusAsync_CreatesNewStatus()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var (playlistId, _) = await SetupPlaylistAsync(context);
            var videoId = "video_123";
            var service = new VideoStatusService(context);

            // Act
            var result = await service.UpdateStatusAsync(playlistId, videoId, "Seen");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(videoId, result.VideoId);
            Assert.Equal(VideoStatusEnum.Seen, result.Status);
            Assert.Equal(playlistId, result.PlaylistId);
        }

        [Fact]
        public async Task UpdateStatusAsync_UpdatesExistingStatus()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var (playlistId, _) = await SetupPlaylistAsync(context);
            var videoId = "video_123";

            var status = new VideoStatus
            {
                PlaylistId = playlistId,
                VideoId = videoId,
                Status = VideoStatusEnum.New,
                FirstSeenUtc = DateTime.UtcNow.AddHours(-1),
                LastUpdatedUtc = DateTime.UtcNow.AddHours(-1)
            };
            context.VideoStatuses.Add(status);
            await context.SaveChangesAsync();

            var service = new VideoStatusService(context);

            // Act
            var result = await service.UpdateStatusAsync(playlistId, videoId, "Saved");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(VideoStatusEnum.Saved, result.Status);
            Assert.True(result.LastUpdatedUtc > DateTime.UtcNow.AddSeconds(-5));
        }

        [Fact]
        public async Task UpdateStatusAsync_WithRejectReason()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var (playlistId, _) = await SetupPlaylistAsync(context);
            var videoId = "video_123";
            var rejectReason = "Duplicate content";
            var service = new VideoStatusService(context);

            // Act
            var result = await service.UpdateStatusAsync(playlistId, videoId, "Rejected", rejectReason);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(VideoStatusEnum.Rejected, result.Status);
            Assert.Equal(rejectReason, result.RejectReason);
        }

        [Fact]
        public async Task UpdateStatusAsync_InvalidStatus_ThrowsException()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var (playlistId, _) = await SetupPlaylistAsync(context);
            var service = new VideoStatusService(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.UpdateStatusAsync(playlistId, "video_123", "InvalidStatus"));
        }

        [Fact]
        public async Task UpdateStatusAsync_ValidStatuses()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var (playlistId, _) = await SetupPlaylistAsync(context);
            var videoId = "video_123";
            var service = new VideoStatusService(context);
            var validStatuses = new[] { "New", "Seen", "Saved", "Rejected" };

            // Act & Assert
            foreach (var status in validStatuses)
            {
                var result = await service.UpdateStatusAsync(playlistId, videoId, status);
                Assert.Equal((VideoStatusEnum)Enum.Parse(typeof(VideoStatusEnum), status), result.Status);
            }
        }

        // GetExistingVideoIdsAsync Tests
        [Fact]
        public async Task GetExistingVideoIdsAsync_ReturnsAllVideoIds()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var (playlistId, _) = await SetupPlaylistAsync(context);
            var videoIds = new[] { "video_1", "video_2", "video_3" };

            foreach (var videoId in videoIds)
            {
                context.VideoStatuses.Add(new VideoStatus
                {
                    PlaylistId = playlistId,
                    VideoId = videoId,
                    Status = VideoStatusEnum.Seen,
                    FirstSeenUtc = DateTime.UtcNow,
                    LastUpdatedUtc = DateTime.UtcNow
                });
            }
            await context.SaveChangesAsync();

            var service = new VideoStatusService(context);

            // Act
            var result = await service.GetExistingVideoIdsAsync(playlistId);

            // Assert
            Assert.Equal(3, result.Count());
            foreach (var videoId in videoIds)
            {
                Assert.Contains(videoId, result);
            }
        }

        [Fact]
        public async Task GetExistingVideoIdsAsync_EmptyPlaylist_ReturnsEmpty()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var (playlistId, _) = await SetupPlaylistAsync(context);
            var service = new VideoStatusService(context);

            // Act
            var result = await service.GetExistingVideoIdsAsync(playlistId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetExistingVideoIdsAsync_OnlyReturnsVideoidsForSpecificPlaylist()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var (playlistId1, userId) = await SetupPlaylistAsync(context);

            // Create another playlist
            var playlistId2 = Guid.NewGuid();
            var playlist2 = new Playlist
            {
                Id = playlistId2,
                Name = "Another Playlist",
                OwnerUserId = userId,
                SearchQuery = "test",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };
            context.Playlists.Add(playlist2);
            await context.SaveChangesAsync();

            // Add videos to both playlists
            context.VideoStatuses.Add(new VideoStatus
            {
                PlaylistId = playlistId1,
                VideoId = "video_1",
                Status = VideoStatusEnum.Seen,
                FirstSeenUtc = DateTime.UtcNow,
                LastUpdatedUtc = DateTime.UtcNow
            });
            context.VideoStatuses.Add(new VideoStatus
            {
                PlaylistId = playlistId2,
                VideoId = "video_2",
                Status = VideoStatusEnum.Seen,
                FirstSeenUtc = DateTime.UtcNow,
                LastUpdatedUtc = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var service = new VideoStatusService(context);

            // Act
            var result1 = await service.GetExistingVideoIdsAsync(playlistId1);
            var result2 = await service.GetExistingVideoIdsAsync(playlistId2);

            // Assert
            Assert.Single(result1);
            Assert.Equal("video_1", result1.First());
            Assert.Single(result2);
            Assert.Equal("video_2", result2.First());
        }

        // VideoExistsAsync Tests
        [Fact]
        public async Task VideoExistsAsync_VideoExists_ReturnsTrue()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var (playlistId, _) = await SetupPlaylistAsync(context);
            var videoId = "video_123";

            context.VideoStatuses.Add(new VideoStatus
            {
                PlaylistId = playlistId,
                VideoId = videoId,
                Status = VideoStatusEnum.Seen,
                FirstSeenUtc = DateTime.UtcNow,
                LastUpdatedUtc = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var service = new VideoStatusService(context);

            // Act
            var result = await service.VideoExistsAsync(playlistId, videoId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task VideoExistsAsync_VideoDoesNotExist_ReturnsFalse()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var (playlistId, _) = await SetupPlaylistAsync(context);
            var service = new VideoStatusService(context);

            // Act
            var result = await service.VideoExistsAsync(playlistId, "nonexistent_video");

            // Assert
            Assert.False(result);
        }

        // Status transitions
        [Fact]
        public async Task UpdateStatusAsync_TransitionBetweenAllStates()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var (playlistId, _) = await SetupPlaylistAsync(context);
            var videoId = "video_123";
            var service = new VideoStatusService(context);

            // Act & Assert - Test transition: New -> Seen -> Saved -> Rejected -> Seen
            var transitions = new[] { "New", "Seen", "Saved", "Rejected", "Seen" };
            foreach (var status in transitions)
            {
                var result = await service.UpdateStatusAsync(playlistId, videoId, status);
                Assert.Equal((VideoStatusEnum)Enum.Parse(typeof(VideoStatusEnum), status), result.Status);
            }
        }

        [Fact]
        public async Task UpdateStatusAsync_PreservesFirstSeenUtc()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var (playlistId, _) = await SetupPlaylistAsync(context);
            var videoId = "video_123";
            var service = new VideoStatusService(context);

            // Act - Create status
            var created = await service.UpdateStatusAsync(playlistId, videoId, "New");
            var createdFirstSeen = created.FirstSeenUtc;

            // Wait a bit and update
            await Task.Delay(100);
            var updated = await service.UpdateStatusAsync(playlistId, videoId, "Seen");

            // Assert - FirstSeenUtc should remain the same
            Assert.Equal(createdFirstSeen, updated.FirstSeenUtc);
            Assert.True(updated.LastUpdatedUtc >= createdFirstSeen);
        }
    }
}
