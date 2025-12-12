using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;

namespace AFHPNewsFunctions.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string?> SendEmailWithDelayAsync(string email, string subject, string htmlMessage, int minDelayMS, int maxDelayMS)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(minDelayMS, maxDelayMS)));
            return await SendEmailAsync(email, subject, htmlMessage);
        }

        public async Task<string?> SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (string.IsNullOrWhiteSpace(email)) return "Recipient email is missing.";
            if (string.IsNullOrWhiteSpace(subject)) subject = "(No subject)";
            if (string.IsNullOrWhiteSpace(htmlMessage)) htmlMessage = "This message has no content.";

            var senderEmail = _configuration["Email.SenderEmail"];
            var senderName = _configuration["Email.SenderName"] ?? "AFHP News";
            var smtpServer = _configuration["Email.SmtpServer"];
            var smtpPort = _configuration["Email.SmtpPort"];
            var smtpUser = _configuration["Email.SmtpUsername"];
            var smtpPass = _configuration["Email.SmtpPassword"];

            if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(smtpServer) || string.IsNullOrWhiteSpace(smtpPort))
                return "SMTP configuration is incomplete.";

            Console.WriteLine($"Preparing to send email to {email} via {smtpServer}:{smtpPort}");

            var sender = new MailboxAddress(senderName, senderEmail);
            var message = new MimeMessage
            {
                Sender = sender,
                Subject = subject
            };

            message.To.Add(MailboxAddress.Parse(email));
            message.From.Add(sender);
            message.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };

            using var emailClient = new SmtpClient();
            try
            {
                // ✅ Use SslOnConnect for port 465
                await emailClient.ConnectAsync(smtpServer, Convert.ToInt32(smtpPort), MailKit.Security.SecureSocketOptions.SslOnConnect);
                Console.WriteLine("SMTP connection established.");

                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

                if (!string.IsNullOrWhiteSpace(smtpUser) && !string.IsNullOrWhiteSpace(smtpPass))
                {
                    await emailClient.AuthenticateAsync(smtpUser, smtpPass);
                    Console.WriteLine("SMTP authentication succeeded.");
                }

                await emailClient.SendAsync(message);
                Console.WriteLine("Email sent successfully.");
                await emailClient.DisconnectAsync(true);

                return null; // success
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
                return $"Email sending failed: {ex.Message}";
            }
        }
    }
}
