using Xunit;
using Microsoft.EntityFrameworkCore;
using YouTubeKurator.Api.Data;
using YouTubeKurator.Api.Data.Entities;
using YouTubeKurator.Api.Tools;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace YouTubeKurator.Tests.Data
{
    public class PlaylistMigrationTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task MigrateAllPlaylistsAsync_AssignsOwnUserIdToPlaylists()
        {
            // Arrange
            using var context = CreateInMemoryContext();

            // Create some test playlists without owner
            var playlist1 = new Playlist
            {
                Id = Guid.NewGuid(),
                OwnerUserId = Guid.Empty,
                Name = "Test Playlist 1",
                SearchQuery = "csharp",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            context.Playlists.Add(playlist1);
            await context.SaveChangesAsync();

            // Act
            await PlaylistMigrationTool.MigrateAllPlaylistsAsync(context);

            // Assert
            var migratedPlaylist = await context.Playlists.FindAsync(playlist1.Id);
            Assert.NotNull(migratedPlaylist);
            Assert.Equal(new Guid("00000000-0000-0000-0000-000000000001"), migratedPlaylist.OwnerUserId);
        }

        [Fact]
        public async Task MigrateAllPlaylistsAsync_ConvertSearchQueryToFilters()
        {
            // Arrange
            using var context = CreateInMemoryContext();

            var playlist = new Playlist
            {
                Id = Guid.NewGuid(),
                OwnerUserId = Guid.Empty,
                Name = "Test Playlist",
                SearchQuery = "dotnet",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            // Act
            await PlaylistMigrationTool.MigrateAllPlaylistsAsync(context);

            // Assert
            var migratedPlaylist = await context.Playlists.FindAsync(playlist.Id);
            Assert.NotNull(migratedPlaylist);
            Assert.NotNull(migratedPlaylist.Filters);

            // Parse JSON to verify structure
            using var doc = JsonDocument.Parse(migratedPlaylist.Filters);
            var root = doc.RootElement;
            Assert.True(root.TryGetProperty("themes", out var themes));
            Assert.Equal(1, themes.GetArrayLength());
            Assert.Equal("dotnet", themes[0].GetString());
        }

        [Fact]
        public async Task MigrateAllPlaylistsAsync_SetsDefaultValues()
        {
            // Arrange
            using var context = CreateInMemoryContext();

            var playlist = new Playlist
            {
                Id = Guid.NewGuid(),
                OwnerUserId = Guid.Empty,
                Name = "Test Playlist",
                SearchQuery = "test",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow,
                SortStrategy = "DefaultSort",
                IsPaused = true
            };

            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            // Act
            await PlaylistMigrationTool.MigrateAllPlaylistsAsync(context);

            // Assert
            var migratedPlaylist = await context.Playlists.FindAsync(playlist.Id);
            Assert.NotNull(migratedPlaylist);
            // SortStrategy should be updated to NewestFirst if it was something else
            Assert.Equal("NewestFirst", migratedPlaylist.SortStrategy);
            Assert.False(migratedPlaylist.IsPaused);
        }

        [Fact]
        public async Task MigrateAllPlaylistsAsync_PreservesPlaylistData()
        {
            // Arrange
            using var context = CreateInMemoryContext();

            var playlistId = Guid.NewGuid();
            var playlist = new Playlist
            {
                Id = playlistId,
                OwnerUserId = Guid.Empty,
                Name = "Original Name",
                SearchQuery = "original query",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            var originalCreatedUtc = playlist.CreatedUtc;

            // Act
            await PlaylistMigrationTool.MigrateAllPlaylistsAsync(context);

            // Assert
            var migratedPlaylist = await context.Playlists.FindAsync(playlistId);
            Assert.NotNull(migratedPlaylist);
            Assert.Equal("Original Name", migratedPlaylist.Name);
            Assert.Equal("original query", migratedPlaylist.SearchQuery); // Original SearchQuery preserved
            Assert.Equal(originalCreatedUtc, migratedPlaylist.CreatedUtc);
        }

        [Fact]
        public async Task MigrateAllPlaylistsAsync_CreatesSystemUserIfMissing()
        {
            // Arrange
            using var context = CreateInMemoryContext();

            // Don't add the system user explicitly
            var playlist = new Playlist
            {
                Id = Guid.NewGuid(),
                OwnerUserId = Guid.Empty,
                Name = "Test Playlist",
                SearchQuery = "test",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            // Act
            await PlaylistMigrationTool.MigrateAllPlaylistsAsync(context);

            // Assert
            var systemUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == "system@youtube-kurator.local");
            Assert.NotNull(systemUser);
            Assert.Equal("system@youtube-kurator.local", systemUser.Email);
        }

    }
}
