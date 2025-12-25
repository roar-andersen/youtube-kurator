using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using YouTubeKurator.Api.Services;
using System.Threading.Tasks;

namespace YouTubeKurator.Tests.Services
{
    public class YouTubeServiceTests
    {
        private Mock<IConfiguration> CreateMockConfiguration(string apiKey = "test-key")
        {
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["YouTubeApi:ApiKey"]).Returns(apiKey);
            return configMock;
        }

        [Fact]
        public void Constructor_WithNullApiKey_ThrowsInvalidOperationException()
        {
            // Arrange
            var configMock = CreateMockConfiguration(null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => new YouTubeService(configMock.Object));
        }

        [Fact]
        public void Constructor_WithEmptyApiKey_ThrowsInvalidOperationException()
        {
            // Arrange
            var configMock = CreateMockConfiguration("");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => new YouTubeService(configMock.Object));
        }

        [Fact]
        public async Task SearchVideosAsync_WithEmptyQuery_ReturnsInvalidQueryError()
        {
            // Arrange
            var configMock = CreateMockConfiguration();
            var service = new YouTubeService(configMock.Object);

            // Act
            var (videos, errorType, errorMessage) = await service.SearchVideosAsync("");

            // Assert
            Assert.Equal("InvalidQuery", errorType);
            Assert.Equal("Søkeordet er tomt.", errorMessage);
            Assert.Empty(videos);
        }

        [Fact]
        public async Task SearchVideosAsync_WithNullQuery_ReturnsInvalidQueryError()
        {
            // Arrange
            var configMock = CreateMockConfiguration();
            var service = new YouTubeService(configMock.Object);

            // Act
            var (videos, errorType, errorMessage) = await service.SearchVideosAsync(null);

            // Assert
            Assert.Equal("InvalidQuery", errorType);
            Assert.Equal("Søkeordet er tomt.", errorMessage);
            Assert.Empty(videos);
        }

        [Fact]
        public async Task SearchVideosAsync_WithWhitespaceQuery_ReturnsInvalidQueryError()
        {
            // Arrange
            var configMock = CreateMockConfiguration();
            var service = new YouTubeService(configMock.Object);

            // Act
            var (videos, errorType, errorMessage) = await service.SearchVideosAsync("   ");

            // Assert
            Assert.Equal("InvalidQuery", errorType);
            Assert.Equal("Søkeordet er tomt.", errorMessage);
            Assert.Empty(videos);
        }

        // Note: Real API tests require a valid API key and network connection
        // These tests would fail in CI/CD without proper mocking of the YouTube API client
        // For production, consider using an interface for YouTubeService and mocking the YouTube API client
    }
}
