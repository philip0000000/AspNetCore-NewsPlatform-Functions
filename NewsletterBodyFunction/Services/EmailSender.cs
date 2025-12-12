using AFHP_NewsSite.Models;
using AFHP_NewsSite.Services.Newsletter;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace NewsletterBodyFunction.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly NewsletterOptions _options;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<NewsletterOptions> options, ILogger<EmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendNewsletterAsync(string recipientEmail, NewsletterDigestResult digest)
        {
            if (_options.DryRun)
            {
                _logger.LogInformation("[DryRun] Would send newsletter to {Recipient}", recipientEmail);
                return;
            }

            try
            {
                using var client = new SmtpClient(_options.SmtpServer, _options.SmtpPort)
                {
                    Credentials = new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword),
                    EnableSsl = true
                };

                var message = new MailMessage
                {
                    From = new MailAddress(_options.FromEmail, _options.FromName),
                    Subject = "AFHP Weekly Newsletter",
                    Body = digest.HtmlContent,
                    IsBodyHtml = true
                };

                // Add plain text alternative for clients that don’t support HTML
                if (!string.IsNullOrWhiteSpace(digest.PlainTextContent))
                {
                    var plainView = AlternateView.CreateAlternateViewFromString(digest.PlainTextContent, null, "text/plain");
                    var htmlView = AlternateView.CreateAlternateViewFromString(digest.HtmlContent, null, "text/html");
                    message.AlternateViews.Add(plainView);
                    message.AlternateViews.Add(htmlView);
                }

                message.To.Add(recipientEmail);

                await client.SendMailAsync(message);
                _logger.LogInformation("Newsletter sent to {Recipient}", recipientEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending newsletter to {Recipient}", recipientEmail);
                throw;
            }
        }
    }
}
