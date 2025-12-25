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
    public class WatchLaterServiceTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private async Task<User> CreateUserAsync(AppDbContext context)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                CreatedUtc = DateTime.UtcNow,
                IsActive = true
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        private async Task<Playlist> CreatePlaylistAsync(AppDbContext context, Guid userId)
        {
            var playlist = new Playlist
            {
                Id = Guid.NewGuid(),
                Name = "Test Playlist",
                OwnerUserId = userId,
                SearchQuery = "test",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };
            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();
            return playlist;
        }

        // AddAsync Tests
        [Fact]
        public async Task AddAsync_GlobalWatchLater_CreatesEntry()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var user = await CreateUserAsync(context);
            var videoId = "video_123";
            var service = new WatchLaterService(context);

            // Act
            var result = await service.AddAsync(user.Id, videoId, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(videoId, result.VideoId);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(Guid.Empty, result.PlaylistId);
            Assert.NotEqual(default(DateTime), result.AddedUtc);
        }

        [Fact]
        public async Task AddAsync_PlaylistSpecificWatchLater_CreatesEntry()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var user = await CreateUserAsync(context);
            var playlist = await CreatePlaylistAsync(context, user.Id);
            var videoId = "video_456";
            var service = new WatchLaterService(context);

            // Act
            var result = await service.AddAsync(user.Id, videoId, playlist.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(videoId, result.VideoId);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(playlist.Id, result.PlaylistId);
        }

        [Fact]
        public async Task AddAsync_DuplicateEntry_ReturnsExisting()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var user = await CreateUserAsync(context);
            var videoId = "video_789";
            var service = new WatchLaterService(context);

            // Act - Add twice
            var first = await service.AddAsync(user.Id, videoId, null);
            var second = await service.AddAsync(user.Id, videoId, null);

            // Assert - Should be the same entry
            Assert.Equal(first.VideoId, second.VideoId);
            Assert.Equal(first.UserId, second.UserId);
            Assert.Equal(first.PlaylistId, second.PlaylistId);
            Assert.Equal(first.AddedUtc, second.AddedUtc);
        }

        [Fact]
        public async Task AddAsync_SameVideoMultiplePlaylists_CreatesMultipleEntries()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var user = await CreateUserAsync(context);
            var playlist1 = await CreatePlaylistAsync(context, user.Id);
            var playlist2 = await CreatePlaylistAsync(context, user.Id);
            var videoId = "video_shared";
            var service = new WatchLaterService(context);

            // Act
            var result1 = await service.AddAsync(user.Id, videoId, playlist1.Id);
            var result2 = await service.AddAsync(user.Id, videoId, playlist2.Id);
            var result3 = await service.AddAsync(user.Id, videoId, null); // Global

            // Assert
            // GetWatchLaterAsync without playlistId returns only global watch later
            var globalItems = await service.GetWatchLaterAsync(user.Id);
            Assert.Single(globalItems);
            Assert.NotEqual(result1.PlaylistId, result2.PlaylistId);
            Assert.Equal(Guid.Empty, result3.PlaylistId);
        }

        // GetWatchLaterAsync Tests
        [Fact]
        public async Task GetWatchLaterAsync_GlobalWatchLater_ReturnsGlobalOnly()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var user = await CreateUserAsync(context);
            var playlist = await CreatePlaylistAsync(context, user.Id);
            var service = new WatchLaterService(context);

            // Add videos to different lists
            await service.AddAsync(user.Id, "video_1", null); // Global
            await service.AddAsync(user.Id, "video_2", null); // Global
            await service.AddAsync(user.Id, "video_3", playlist.Id); // Playlist

            // Act
            var result = await service.GetWatchLaterAsync(user.Id, null);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, wl => wl.VideoId == "video_1" && wl.PlaylistId == Guid.Empty);
            Assert.Contains(result, wl => wl.VideoId == "video_2" && wl.PlaylistId == Guid.Empty);
            Assert.DoesNotContain(result, wl => wl.VideoId == "video_3");
        }

        [Fact]
        public async Task GetWatchLaterAsync_PlaylistSpecific_ReturnsPlaylistOnly()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var user = await CreateUserAsync(context);
            var playlist = await CreatePlaylistAsync(context, user.Id);
            var service = new WatchLaterService(context);

            // Add videos to different lists
            await service.AddAsync(user.Id, "video_1", null); // Global
            await service.AddAsync(user.Id, "video_2", playlist.Id); // Playlist
            await service.AddAsync(user.Id, "video_3", playlist.Id); // Playlist

            // Act
            var result = await service.GetWatchLaterAsync(user.Id, playlist.Id);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, wl => wl.VideoId == "video_2" && wl.PlaylistId == playlist.Id);
            Assert.Contains(result, wl => wl.VideoId == "video_3" && wl.PlaylistId == playlist.Id);
            Assert.DoesNotContain(result, wl => wl.VideoId == "video_1");
        }

        [Fact]
        public async Task GetWatchLaterAsync_EmptyList_ReturnsEmpty()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var user = await CreateUserAsync(context);
            var service = new WatchLaterService(context);

            // Act
            var result = await service.GetWatchLaterAsync(user.Id, null);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetWatchLaterAsync_OnlyReturnsUserItems()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var user1 = await CreateUserAsync(context);
            var user2 = await CreateUserAsync(context);
            var service = new WatchLaterService(context);

            // Add videos for both users
            await service.AddAsync(user1.Id, "video_1", null);
            await service.AddAsync(user1.Id, "video_2", null);
            await service.AddAsync(user2.Id, "video_3", null);

            // Act
            var user1Result = await service.GetWatchLaterAsync(user1.Id, null);
            var user2Result = await service.GetWatchLaterAsync(user2.Id, null);

            // Assert
            Assert.Equal(2, user1Result.Count());
            Assert.Single(user2Result);
            Assert.DoesNotContain(user1Result, wl => wl.VideoId == "video_3");
        }

        // RemoveAsync Tests
        [Fact]
        public async Task RemoveAsync_GlobalWatchLater_RemovesEntry()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var user = await CreateUserAsync(context);
            var videoId = "video_remove";
            var service = new WatchLaterService(context);
            await service.AddAsync(user.Id, videoId, null);

            // Act
            var result = await service.RemoveAsync(user.Id, videoId, null);

            // Assert
            Assert.True(result);
            var remaining = await service.GetWatchLaterAsync(user.Id, null);
            Assert.Empty(remaining);
        }

        [Fact]
        public async Task RemoveAsync_PlaylistSpecific_RemovesEntry()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var user = await CreateUserAsync(context);
            var playlist = await CreatePlaylistAsync(context, user.Id);
            var videoId = "video_remove";
            var service = new WatchLaterService(context);
            await service.AddAsync(user.Id, videoId, playlist.Id);

            // Act
            var result = await service.RemoveAsync(user.Id, videoId, playlist.Id);

            // Assert
            Assert.True(result);
            var remaining = await service.GetWatchLaterAsync(user.Id, playlist.Id);
            Assert.Empty(remaining);
        }

        [Fact]
        public async Task RemoveAsync_NonexistentEntry_ReturnsFalse()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var user = await CreateUserAsync(context);
            var service = new WatchLaterService(context);

            // Act
            var result = await service.RemoveAsync(user.Id, "nonexistent", null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveAsync_RemovesOnlySpecificEntry()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var user = await CreateUserAsync(context);
            var playlist = await CreatePlaylistAsync(context, user.Id);
            var videoId = "video_shared";
            var service = new WatchLaterService(context);

            // Add to both global and playlist
            await service.AddAsync(user.Id, videoId, null);
            await service.AddAsync(user.Id, videoId, playlist.Id);

            // Act - Remove from playlist only
            var result = await service.RemoveAsync(user.Id, videoId, playlist.Id);

            // Assert
            Assert.True(result);
            var globalRemaining = await service.GetWatchLaterAsync(user.Id, null);
            var playlistRemaining = await service.GetWatchLaterAsync(user.Id, playlist.Id);
            Assert.Single(globalRemaining);
            Assert.Empty(playlistRemaining);
        }

        // IsInWatchLaterAsync Tests
        [Fact]
        public async Task IsInWatchLaterAsync_VideoExists_ReturnsTrue()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var user = await CreateUserAsync(context);
            var videoId = "video_check";
            var service = new WatchLaterService(context);
            await service.AddAsync(user.Id, videoId, null);

            // Act
            var result = await service.IsInWatchLaterAsync(user.Id, videoId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsInWatchLaterAsync_VideoNotExists_ReturnsFalse()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var user = await CreateUserAsync(context);
            var service = new WatchLaterService(context);

            // Act
            var result = await service.IsInWatchLaterAsync(user.Id, "nonexistent");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsInWatchLaterAsync_ChecksAnyPlaylistOrGlobal()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var user = await CreateUserAsync(context);
            var playlist = await CreatePlaylistAsync(context, user.Id);
            var videoId = "video_check";
            var service = new WatchLaterService(context);

            // Add to playlist
            await service.AddAsync(user.Id, videoId, playlist.Id);

            // Act - IsInWatchLaterAsync checks all lists
            var result = await service.IsInWatchLaterAsync(user.Id, videoId);

            // Assert
            Assert.True(result);
        }

        // Edge cases
        [Fact]
        public async Task RemoveAsync_WrongPlaylistId_ReturnsFalse()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var user = await CreateUserAsync(context);
            var playlist1 = await CreatePlaylistAsync(context, user.Id);
            var playlist2 = await CreatePlaylistAsync(context, user.Id);
            var videoId = "video_test";
            var service = new WatchLaterService(context);

            // Add to playlist1
            await service.AddAsync(user.Id, videoId, playlist1.Id);

            // Act - Try to remove from playlist2
            var result = await service.RemoveAsync(user.Id, videoId, playlist2.Id);

            // Assert
            Assert.False(result);
            var stillInPlaylist1 = await service.GetWatchLaterAsync(user.Id, playlist1.Id);
            Assert.Single(stillInPlaylist1);
        }

        [Fact]
        public async Task GetWatchLaterAsync_NonexistentPlaylist_ReturnsEmpty()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var user = await CreateUserAsync(context);
            var fakePlaylistId = Guid.NewGuid();
            var service = new WatchLaterService(context);

            // Act
            var result = await service.GetWatchLaterAsync(user.Id, fakePlaylistId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddAsync_EmptyVideoId_CreatesEntry()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var user = await CreateUserAsync(context);
            var service = new WatchLaterService(context);

            // Act
            var result = await service.AddAsync(user.Id, "", null);

            // Assert - Service doesn't validate, controller does
            Assert.NotNull(result);
            Assert.Empty(result.VideoId);
        }
    }
}
