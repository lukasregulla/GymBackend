using GymBackend.Interfaces;
using Resend;

namespace GymBackend.Services
{
    public class ResendEmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly ILogger<ResendEmailService> _logger;

        public ResendEmailService(IResend resend, ILogger<ResendEmailService> logger)
        {
            _resend = resend;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            var message = new EmailMessage
            {
                From = "onboarding@resend.dev",
                Subject = subject,
                HtmlBody = body
            };
            message.To.Add(to);

            try
            {
                await _resend.EmailSendAsync(message);
                _logger.LogInformation("Email sent to {To} via Resend", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resend failure sending to {To}", to);
                throw;
            }
        }
    }
}
