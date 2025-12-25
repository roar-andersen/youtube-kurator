using System.Threading.Tasks;

namespace YouTubeKurator.Api.Services
{
    public interface IEmailService
    {
        Task SendAuthCodeAsync(string toEmail, string code);
    }
}
