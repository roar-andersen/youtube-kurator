using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace YouTubeKurator.Api.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendAuthCodeAsync(string toEmail, string code)
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"];
                var smtpPort = _configuration["Email:SmtpPort"];
                var smtpUser = _configuration["Email:SmtpUser"];
                var smtpPassword = _configuration["Email:SmtpPassword"];
                var fromAddress = _configuration["Email:FromAddress"];

                // If email is not configured, just log the code
                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(fromAddress))
                {
                    _logger.LogInformation($"Auth code for {toEmail}: {code}");
                    return;
                }

                if (!int.TryParse(smtpPort, out var port))
                {
                    port = 587;
                }

                using (var client = new SmtpClient(smtpHost, port))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(smtpUser, smtpPassword);

                    var mailMessage = new MailMessage(fromAddress, toEmail)
                    {
                        Subject = "YouTube Kurator - Login Code",
                        Body = $"Your login code is: {code}\n\nThis code expires in 15 minutes.",
                        IsBodyHtml = false
                    };

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation($"Auth code email sent to {toEmail}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send auth code email to {toEmail}");
                // Don't throw - let the auth flow continue even if email fails
            }
        }
    }
}
