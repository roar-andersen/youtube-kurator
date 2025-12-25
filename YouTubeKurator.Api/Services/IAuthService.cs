using System;
using System.Threading.Tasks;

namespace YouTubeKurator.Api.Services
{
    public interface IAuthService
    {
        Task<(bool success, string message)> SendAuthCodeAsync(string email);
        Task<(bool success, string tokenOrError, string userId)> VerifyCodeAsync(string email, string code);
    }
}
