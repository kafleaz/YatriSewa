using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace YatriSewa.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpClient _smtpClient;
        private readonly string _fromEmail;

        // Inject configuration to retrieve email settings
        public EmailService(IConfiguration configuration)
        {
            _fromEmail = configuration["EmailSettings:FromEmail"] ?? throw new ArgumentNullException("FromEmail cannot be null in the configuration.");
            var smtpHost = configuration["EmailSettings:SmtpHost"] ?? throw new ArgumentNullException("SmtpHost cannot be null in the configuration.");
            var smtpUsername = configuration["EmailSettings:SmtpUsername"] ?? throw new ArgumentNullException("SmtpUsername cannot be null in the configuration.");
            var smtpPassword = configuration["EmailSettings:SmtpPassword"] ?? throw new ArgumentNullException("SmtpPassword cannot be null in the configuration.");
            var enableSsl = bool.Parse(configuration["EmailSettings:EnableSsl"] ?? "true");

            _smtpClient = new SmtpClient(smtpHost)
            {
                Port = int.Parse(configuration["EmailSettings:Port"] ?? "587"),
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = enableSsl
            };
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var mailMessage = new MailMessage(_fromEmail, email, subject, message);

            try
            {
                await _smtpClient.SendMailAsync(mailMessage);
            }
            catch (SmtpException ex)
            {
                // Log the exception or handle it accordingly
                Console.WriteLine($"SMTP Error: {ex.Message}");
                // Optionally, rethrow the exception to handle it higher up in the call stack
                throw;
            }
        }
    }
}
