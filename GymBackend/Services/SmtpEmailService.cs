using GymBackend.Interfaces;
using System.Net;
using System.Net.Mail;

namespace GymBackend.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            var host = _configuration["Smtp:Host"]!;
            var port = int.Parse(_configuration["Smtp:Port"]!);
            var user = _configuration["Smtp:User"]!;
            var pass = _configuration["Smtp:Pass"]!;
            var from = _configuration["Smtp:From"]!;

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(user, pass),
                EnableSsl = true
            };

            var message = new MailMessage(from, to, subject, body);
            message.IsBodyHtml = true;
            await client.SendMailAsync(message);
        }
    }
}
