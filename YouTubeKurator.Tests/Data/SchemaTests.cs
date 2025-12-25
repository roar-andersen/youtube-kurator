using Xunit;
using Microsoft.EntityFrameworkCore;
using YouTubeKurator.Api.Data;
using YouTubeKurator.Api.Data.Entities;
using System;
using System.Threading.Tasks;

namespace YouTubeKurator.Tests.Data
{
    public class SchemaTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task User_CanBeCreated()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                CreatedUtc = DateTime.UtcNow,
                IsActive = true
            };

            // Act
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Assert
            var savedUser = await context.Users.FindAsync(userId);
            Assert.NotNull(savedUser);
            Assert.Equal("test@example.com", savedUser.Email);
            Assert.True(savedUser.IsActive);
        }

        [Fact]
        public async Task VideoStatus_CanBeCreated()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var playlistId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // First create user and playlist
            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                CreatedUtc = DateTime.UtcNow,
                IsActive = true
            };

            var playlist = new Playlist
            {
                Id = playlistId,
                OwnerUserId = userId,
                Name = "Test Playlist",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            context.Users.Add(user);
            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            var videoStatus = new VideoStatus
            {
                PlaylistId = playlistId,
                VideoId = "dQw4w9WgXcQ",
                Status = VideoStatusEnum.Seen,
                FirstSeenUtc = DateTime.UtcNow,
                LastUpdatedUtc = DateTime.UtcNow
            };

            // Act
            context.VideoStatuses.Add(videoStatus);
            await context.SaveChangesAsync();

            // Assert
            var savedVideoStatus = await context.VideoStatuses
                .FirstOrDefaultAsync(vs => vs.PlaylistId == playlistId && vs.VideoId == "dQw4w9WgXcQ");
            Assert.NotNull(savedVideoStatus);
            Assert.Equal(VideoStatusEnum.Seen, savedVideoStatus.Status);
        }

        [Fact]
        public async Task WatchLater_CanBeCreated()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var userId = Guid.NewGuid();
            var playlistId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                CreatedUtc = DateTime.UtcNow,
                IsActive = true
            };

            var playlist = new Playlist
            {
                Id = playlistId,
                OwnerUserId = userId,
                Name = "Test Playlist",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            context.Users.Add(user);
            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            var watchLater = new WatchLater
            {
                UserId = userId,
                VideoId = "dQw4w9WgXcQ",
                PlaylistId = playlistId,
                AddedUtc = DateTime.UtcNow
            };

            // Act
            context.WatchLater.Add(watchLater);
            await context.SaveChangesAsync();

            // Assert
            var savedWatchLater = await context.WatchLater
                .FirstOrDefaultAsync(wl => wl.UserId == userId && wl.VideoId == "dQw4w9WgXcQ" && wl.PlaylistId == playlistId);
            Assert.NotNull(savedWatchLater);
            Assert.Equal(playlistId, savedWatchLater.PlaylistId);
        }

        [Fact]
        public async Task Playlist_HasAllV2Fields()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var playlistId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                CreatedUtc = DateTime.UtcNow,
                IsActive = true
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var playlist = new Playlist
            {
                Id = playlistId,
                OwnerUserId = userId,
                Name = "Advanced Playlist",
                SearchQuery = "csharp", // v1 field for backward compatibility
                Description = "Test Description",
                Filters = "{\"type\":\"duration\",\"value\":10}",
                SortStrategy = "MostViewed",
                DiscoveryProfile = "{\"mode\":\"enabled\"}",
                IsPaused = false,
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            // Act
            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            // Assert
            var savedPlaylist = await context.Playlists.FindAsync(playlistId);
            Assert.NotNull(savedPlaylist);
            Assert.Equal(userId, savedPlaylist.OwnerUserId);
            Assert.Equal("csharp", savedPlaylist.SearchQuery); // v1 backward compatibility
            Assert.Equal("Test Description", savedPlaylist.Description);
            Assert.Equal("{\"type\":\"duration\",\"value\":10}", savedPlaylist.Filters);
            Assert.Equal("MostViewed", savedPlaylist.SortStrategy);
            Assert.Equal("{\"mode\":\"enabled\"}", savedPlaylist.DiscoveryProfile);
            Assert.False(savedPlaylist.IsPaused);
        }

        [Fact]
        public async Task User_CanHaveMultiplePlaylistsAndWatchLaterItems()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                CreatedUtc = DateTime.UtcNow,
                IsActive = true
            };

            var playlist1 = new Playlist
            {
                Id = Guid.NewGuid(),
                OwnerUserId = userId,
                Name = "Playlist 1",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            var playlist2 = new Playlist
            {
                Id = Guid.NewGuid(),
                OwnerUserId = userId,
                Name = "Playlist 2",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            context.Users.Add(user);
            context.Playlists.Add(playlist1);
            context.Playlists.Add(playlist2);
            await context.SaveChangesAsync();

            var watchLater1 = new WatchLater
            {
                UserId = userId,
                VideoId = "video1",
                PlaylistId = playlist1.Id,
                AddedUtc = DateTime.UtcNow
            };

            var watchLater2 = new WatchLater
            {
                UserId = userId,
                VideoId = "video2",
                PlaylistId = playlist2.Id,
                AddedUtc = DateTime.UtcNow
            };

            // Act
            context.WatchLater.Add(watchLater1);
            context.WatchLater.Add(watchLater2);
            await context.SaveChangesAsync();

            // Assert
            var savedUser = await context.Users
                .Include(u => u.Playlists)
                .Include(u => u.WatchLaterItems)
                .FirstOrDefaultAsync(u => u.Id == userId);

            Assert.NotNull(savedUser);
            Assert.Equal(2, savedUser.Playlists.Count);
            Assert.Equal(2, savedUser.WatchLaterItems.Count);
        }

        [Fact]
        public async Task VideoStatus_WithRejectReason()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var playlistId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                CreatedUtc = DateTime.UtcNow,
                IsActive = true
            };

            var playlist = new Playlist
            {
                Id = playlistId,
                OwnerUserId = userId,
                Name = "Test Playlist",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            context.Users.Add(user);
            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            var videoStatus = new VideoStatus
            {
                PlaylistId = playlistId,
                VideoId = "rejected123",
                Status = VideoStatusEnum.Rejected,
                FirstSeenUtc = DateTime.UtcNow,
                LastUpdatedUtc = DateTime.UtcNow,
                RejectReason = "Duplicate content"
            };

            // Act
            context.VideoStatuses.Add(videoStatus);
            await context.SaveChangesAsync();

            // Assert
            var savedVideoStatus = await context.VideoStatuses
                .FirstOrDefaultAsync(vs => vs.PlaylistId == playlistId && vs.VideoId == "rejected123");
            Assert.NotNull(savedVideoStatus);
            Assert.Equal(VideoStatusEnum.Rejected, savedVideoStatus.Status);
            Assert.Equal("Duplicate content", savedVideoStatus.RejectReason);
        }

        [Fact]
        public async Task DatabaseCanBeCreatedFromScratch()
        {
            // Arrange & Act
            using var context = CreateInMemoryContext();
            await context.Database.EnsureCreatedAsync();

            // Assert
            var canConnect = await context.Database.CanConnectAsync();
            Assert.True(canConnect);
        }
    }
}
