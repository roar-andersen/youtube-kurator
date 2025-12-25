using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using YouTubeKurator.Api.Controllers;
using YouTubeKurator.Api.Data;
using YouTubeKurator.Api.Data.Entities;
using YouTubeKurator.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        private IFilterService CreateFilterService()
        {
            return new FilterService();
        }

        private ISortingService CreateSortingService()
        {
            return new SortingService();
        }

        [Fact]
        public async Task GetPlaylists_WithNoPlaylists_ReturnsEmptyList()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var filterService = CreateFilterService();
            var sortingService = CreateSortingService();
            var controller = new PlaylistsController(context, null, filterService, sortingService);

            // Act
            var result = await controller.GetPlaylists();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var playlists = Assert.IsAssignableFrom<List<Playlist>>(okResult.Value);
            Assert.Empty(playlists);
        }

        [Fact]
        public async Task GetPlaylists_ReturnsAllPlaylists()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var playlist1 = new Playlist
            {
                Id = Guid.NewGuid(),
                Name = "Test Playlist 1",
                SearchQuery = "test1",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };
            var playlist2 = new Playlist
            {
                Id = Guid.NewGuid(),
                Name = "Test Playlist 2",
                SearchQuery = "test2",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };
            context.Playlists.AddRange(playlist1, playlist2);
            await context.SaveChangesAsync();

            var controller = new PlaylistsController(context, null, CreateFilterService(), CreateSortingService());

            // Act
            var result = await controller.GetPlaylists();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var playlists = Assert.IsAssignableFrom<List<Playlist>>(okResult.Value);
            Assert.Equal(2, playlists.Count);
        }

        [Fact]
        public async Task GetPlaylist_WithValidId_ReturnsPlaylist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var playlistId = Guid.NewGuid();
            var playlist = new Playlist
            {
                Id = playlistId,
                Name = "Test Playlist",
                SearchQuery = "test",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };
            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            var controller = new PlaylistsController(context, null, CreateFilterService(), CreateSortingService());

            // Act
            var result = await controller.GetPlaylist(playlistId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPlaylist = Assert.IsType<Playlist>(okResult.Value);
            Assert.Equal(playlistId, returnedPlaylist.Id);
            Assert.Equal("Test Playlist", returnedPlaylist.Name);
        }

        [Fact]
        public async Task GetPlaylist_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var controller = new PlaylistsController(context, null, CreateFilterService(), CreateSortingService());

            // Act
            var result = await controller.GetPlaylist(Guid.NewGuid());

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
        }

        [Fact]
        public async Task CreatePlaylist_WithValidData_CreatesPlaylist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var controller = new PlaylistsController(context, null, CreateFilterService(), CreateSortingService());
            var request = new CreatePlaylistRequest
            {
                Name = "New Playlist",
                SearchQuery = "new test"
            };

            // Act
            var result = await controller.CreatePlaylist(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var createdPlaylist = Assert.IsType<Playlist>(createdResult.Value);
            Assert.Equal("New Playlist", createdPlaylist.Name);
            Assert.Equal("new test", createdPlaylist.SearchQuery);

            var playlists = await context.Playlists.ToListAsync();
            Assert.Single(playlists);
            Assert.Equal("New Playlist", playlists[0].Name);
        }

        [Fact]
        public async Task CreatePlaylist_WithNullRequest_ReturnsBadRequest()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var controller = new PlaylistsController(context, null, CreateFilterService(), CreateSortingService());

            // Act
            var result = await controller.CreatePlaylist(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task CreatePlaylist_WithEmptyName_ReturnsBadRequest()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var controller = new PlaylistsController(context, null, CreateFilterService(), CreateSortingService());
            var request = new CreatePlaylistRequest
            {
                Name = "",
                SearchQuery = "test"
            };

            // Act
            var result = await controller.CreatePlaylist(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task CreatePlaylist_WithEmptySearchQuery_ReturnsBadRequest()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var controller = new PlaylistsController(context, null, CreateFilterService(), CreateSortingService());
            var request = new CreatePlaylistRequest
            {
                Name = "Test",
                SearchQuery = ""
            };

            // Act
            var result = await controller.CreatePlaylist(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task UpdatePlaylist_WithValidData_UpdatesPlaylist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var playlistId = Guid.NewGuid();
            var playlist = new Playlist
            {
                Id = playlistId,
                Name = "Old Name",
                SearchQuery = "old query",
                CreatedUtc = DateTime.UtcNow.AddDays(-1),
                UpdatedUtc = DateTime.UtcNow.AddDays(-1)
            };
            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            var controller = new PlaylistsController(context, null, CreateFilterService(), CreateSortingService());
            var request = new UpdatePlaylistRequest
            {
                Name = "New Name",
                SearchQuery = "new query"
            };

            // Act
            var result = await controller.UpdatePlaylist(playlistId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedPlaylist = Assert.IsType<Playlist>(okResult.Value);
            Assert.Equal("New Name", updatedPlaylist.Name);
            Assert.Equal("new query", updatedPlaylist.SearchQuery);

            var dbPlaylist = await context.Playlists.FindAsync(playlistId);
            Assert.Equal("New Name", dbPlaylist.Name);
            Assert.Equal("new query", dbPlaylist.SearchQuery);
        }

        [Fact]
        public async Task UpdatePlaylist_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var controller = new PlaylistsController(context, null, CreateFilterService(), CreateSortingService());
            var request = new UpdatePlaylistRequest
            {
                Name = "New Name",
                SearchQuery = "new query"
            };

            // Act
            var result = await controller.UpdatePlaylist(Guid.NewGuid(), request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
        }

        [Fact]
        public async Task UpdatePlaylist_WithEmptyName_ReturnsBadRequest()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var controller = new PlaylistsController(context, null, CreateFilterService(), CreateSortingService());
            var request = new UpdatePlaylistRequest
            {
                Name = "",
                SearchQuery = "test"
            };

            // Act
            var result = await controller.UpdatePlaylist(Guid.NewGuid(), request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task DeletePlaylist_WithValidId_DeletesPlaylist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var playlistId = Guid.NewGuid();
            var playlist = new Playlist
            {
                Id = playlistId,
                Name = "Test Playlist",
                SearchQuery = "test",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };
            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            var discoveryServiceMock = new Mock<IDiscoveryService>();
            var controller = new PlaylistsController(context, null, CreateFilterService(), CreateSortingService(), discoveryServiceMock.Object);

            // Act
            var result = await controller.DeletePlaylist(playlistId);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var dbPlaylist = await context.Playlists.FindAsync(playlistId);
            Assert.Null(dbPlaylist);
        }

        [Fact]
        public async Task DeletePlaylist_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var discoveryServiceMock = new Mock<IDiscoveryService>();
            var controller = new PlaylistsController(context, null, CreateFilterService(), CreateSortingService(), discoveryServiceMock.Object);

            // Act
            var result = await controller.DeletePlaylist(Guid.NewGuid());

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
        }

        [Fact]
        public async Task RefreshPlaylist_WithValidId_ReturnsVideos()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var playlistId = Guid.NewGuid();
            var playlist = new Playlist
            {
                Id = playlistId,
                Name = "Test Playlist",
                SearchQuery = "test",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };
            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            var testVideos = new List<Video>
            {
                new Video
                {
                    VideoId = "test123",
                    Title = "Test Video",
                    ChannelName = "Test Channel",
                    ThumbnailUrl = "http://test.com/thumb.jpg",
                    Duration = TimeSpan.FromMinutes(5),
                    PublishedAt = DateTime.UtcNow,
                    ViewCount = 1000
                }
            };

            var cacheServiceMock = new Mock<CacheService>(MockBehavior.Strict, context, null);
            cacheServiceMock.Setup(x => x.GetOrFetchVideosAsync("test"))
                .ReturnsAsync((testVideos, false, DateTime.UtcNow.AddHours(1), null, null));

            var discoveryServiceMock = new Mock<IDiscoveryService>();
            discoveryServiceMock.Setup(x => x.SelectVideosWithDiscoveryAsync(It.IsAny<IEnumerable<Video>>(), It.IsAny<Playlist>(), It.IsAny<int>()))
                .ReturnsAsync(testVideos);

            var controller = new PlaylistsController(context, cacheServiceMock.Object, CreateFilterService(), CreateSortingService(), discoveryServiceMock.Object);

            // Act
            var result = await controller.RefreshPlaylist(playlistId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task RefreshPlaylist_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var cacheServiceMock = new Mock<CacheService>(MockBehavior.Strict, context, null);
            var discoveryServiceMock = new Mock<IDiscoveryService>();
            var controller = new PlaylistsController(context, cacheServiceMock.Object, CreateFilterService(), CreateSortingService(), discoveryServiceMock.Object);

            // Act
            var result = await controller.RefreshPlaylist(Guid.NewGuid());

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
        }
    }
}
