using System;

namespace YouTubeKurator.Api.Services
{
    public interface IJwtService
    {
        string GenerateToken(Guid userId, string email);
        Guid? ValidateToken(string token);
    }
}
