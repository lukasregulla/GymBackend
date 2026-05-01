using GymBackend.Interfaces;
using System.Net;
using System.Net.Mail;

namespace GymBackend.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            var host = _configuration["Smtp:Host"]!;
            var port = int.Parse(_configuration["Smtp:Port"]!);
            var user = _configuration["Smtp:User"]!;
            var pass = _configuration["Smtp:Pass"]!;
            var from = _configuration["Smtp:From"]!;

            try
            {
                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(user, pass),
                    EnableSsl = true
                };
                using var message = new MailMessage(from, to, subject, body) { IsBodyHtml = true };
                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent to {To} via {Host}:{Port}", to, host, port);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP failure sending to {To} via {Host}:{Port}", to, host, port);
                throw;
            }
        }
    }
}
