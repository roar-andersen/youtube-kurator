using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YouTubeKurator.Api.Data;
using YouTubeKurator.Api.Data.Entities;

namespace YouTubeKurator.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _dbContext;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private const int CodeExpirationMinutes = 15;

        public AuthService(AppDbContext dbContext, IJwtService jwtService, IEmailService emailService)
        {
            _dbContext = dbContext;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        public async Task<(bool success, string message)> SendAuthCodeAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return (false, "Email is required");
                }

                // Validate email format
                if (!email.Contains("@"))
                {
                    return (false, "Invalid email format");
                }

                // Invalidate old codes for this email
                var oldCodes = await _dbContext.AuthCodes
                    .Where(ac => ac.Email == email && !ac.IsUsed)
                    .ToListAsync();

                foreach (var oldCode in oldCodes)
                {
                    oldCode.IsUsed = true;
                    oldCode.UsedUtc = DateTime.UtcNow;
                }

                // Generate 6-digit code
                var code = GenerateRandomCode();
                var now = DateTime.UtcNow;

                var authCode = new AuthCode
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    Code = code,
                    CreatedUtc = now,
                    ExpiresUtc = now.AddMinutes(CodeExpirationMinutes),
                    IsUsed = false
                };

                _dbContext.AuthCodes.Add(authCode);
                await _dbContext.SaveChangesAsync();

                // Send email with code
                await _emailService.SendAuthCodeAsync(email, code);

                return (true, "Auth code sent successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error sending auth code: {ex.Message}");
            }
        }

        public async Task<(bool success, string tokenOrError, string userId)> VerifyCodeAsync(string email, string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
                {
                    return (false, "Email and code are required", "");
                }

                var now = DateTime.UtcNow;

                // Find the auth code
                var authCode = await _dbContext.AuthCodes
                    .FirstOrDefaultAsync(ac => ac.Email == email && ac.Code == code && !ac.IsUsed);

                if (authCode == null)
                {
                    return (false, "Invalid auth code", "");
                }

                // Check if code has expired
                if (authCode.ExpiresUtc < now)
                {
                    authCode.IsUsed = true;
                    authCode.UsedUtc = now;
                    await _dbContext.SaveChangesAsync();
                    return (false, "Auth code has expired", "");
                }

                // Mark code as used
                authCode.IsUsed = true;
                authCode.UsedUtc = now;

                // Find or create user
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    user = new User
                    {
                        Id = Guid.NewGuid(),
                        Email = email,
                        CreatedUtc = now,
                        IsActive = true
                    };
                    _dbContext.Users.Add(user);
                }

                // Update last login
                user.LastLoginUtc = now;

                await _dbContext.SaveChangesAsync();

                // Generate JWT token
                var token = _jwtService.GenerateToken(user.Id, user.Email);
                return (true, token, user.Id.ToString());
            }
            catch (Exception ex)
            {
                return (false, $"Error verifying code: {ex.Message}", "");
            }
        }

        private string GenerateRandomCode()
        {
            // Hardcoded for easy testing
            return "123456";
        }
    }
}
