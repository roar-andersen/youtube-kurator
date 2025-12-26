using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using YouTubeKurator.Api.Data;
using YouTubeKurator.Api.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YouTubeKurator.Tests.Services
{
    public class AuthServiceTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private IJwtService CreateJwtService()
        {
            var configDict = new Dictionary<string, string?>
            {
                { "Jwt:SecretKey", "this-is-a-32-character-secret-key-for-jwt!!!" },
                { "Jwt:Issuer", "YouTubeKurator" },
                { "Jwt:Audience", "YouTubeKurator" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();
            return new JwtService(configuration);
        }

        [Fact]
        public async Task SendAuthCodeAsync_WithValidEmail_CreatesCodeInDatabase()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var jwtService = CreateJwtService();
            var emailService = new MockEmailService();
            var authService = new AuthService(context, jwtService, emailService);

            // Act
            var result = await authService.SendAuthCodeAsync("test@example.com");

            // Assert
            Assert.True(result.success);
            var authCode = await context.AuthCodes.FirstOrDefaultAsync(ac => ac.Email == "test@example.com");
            Assert.NotNull(authCode);
            Assert.NotEmpty(authCode.Code);
            Assert.False(authCode.IsUsed);
        }

        [Fact]
        public async Task SendAuthCodeAsync_WithEmptyEmail_ReturnsFalse()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var jwtService = CreateJwtService();
            var emailService = new MockEmailService();
            var authService = new AuthService(context, jwtService, emailService);

            // Act
            var result = await authService.SendAuthCodeAsync("");

            // Assert
            Assert.False(result.success);
        }

        [Fact]
        public async Task VerifyCodeAsync_WithValidCode_ReturnsToken()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var jwtService = CreateJwtService();
            var emailService = new MockEmailService();
            var authService = new AuthService(context, jwtService, emailService);

            // First send a code
            var sendResult = await authService.SendAuthCodeAsync("test@example.com");
            Assert.True(sendResult.success);

            // Get the code from database
            var authCode = await context.AuthCodes.FirstOrDefaultAsync(ac => ac.Email == "test@example.com");
            Assert.NotNull(authCode);

            // Act - Verify the code
            var verifyResult = await authService.VerifyCodeAsync("test@example.com", authCode.Code);

            // Assert
            Assert.True(verifyResult.success);
            Assert.NotEmpty(verifyResult.tokenOrError);

            // Check user was created
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
            Assert.NotNull(user);
        }

        [Fact]
        public async Task VerifyCodeAsync_WithInvalidCode_ReturnsFalse()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var jwtService = CreateJwtService();
            var emailService = new MockEmailService();
            var authService = new AuthService(context, jwtService, emailService);

            // First send a code
            await authService.SendAuthCodeAsync("test@example.com");

            // Act - Verify with wrong code
            var result = await authService.VerifyCodeAsync("test@example.com", "000000");

            // Assert
            Assert.False(result.success);
        }

        [Fact]
        public async Task VerifyCodeAsync_WithExpiredCode_ReturnsFalse()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var jwtService = CreateJwtService();
            var emailService = new MockEmailService();
            var authService = new AuthService(context, jwtService, emailService);

            // Send code
            await authService.SendAuthCodeAsync("test@example.com");

            // Manually expire the code
            var authCode = await context.AuthCodes.FirstOrDefaultAsync(ac => ac.Email == "test@example.com");
            authCode.ExpiresUtc = DateTime.UtcNow.AddHours(-1); // Set to expired
            await context.SaveChangesAsync();

            // Act - Verify expired code
            var result = await authService.VerifyCodeAsync("test@example.com", authCode.Code);

            // Assert
            Assert.False(result.success);
        }

        [Fact]
        public async Task VerifyCodeAsync_CreatesUserIfNotExists()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var jwtService = CreateJwtService();
            var emailService = new MockEmailService();
            var authService = new AuthService(context, jwtService, emailService);

            // Send code
            await authService.SendAuthCodeAsync("newuser@example.com");

            // Get code
            var authCode = await context.AuthCodes.FirstOrDefaultAsync(ac => ac.Email == "newuser@example.com");

            // Act - Verify code (should create user)
            var verifyResult = await authService.VerifyCodeAsync("newuser@example.com", authCode.Code);

            // Assert
            Assert.True(verifyResult.success);
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "newuser@example.com");
            Assert.NotNull(user);
            Assert.Equal("newuser@example.com", user.Email);
            Assert.NotNull(user.LastLoginUtc);
        }
    }

    public class MockEmailService : IEmailService
    {
        public Task SendAuthCodeAsync(string toEmail, string code)
        {
            // Mock: Just return success
            return Task.CompletedTask;
        }
    }
}
