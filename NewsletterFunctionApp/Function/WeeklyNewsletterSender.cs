using AFHP_NewsSite.Services;
using AFHP_NewsSite.Services.Claims_Roles;
using AFHP_NewsSite.ViewModels;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NewsletterFunctionApp.Services.Newsletter;

namespace NewsletterFunctionApp.Functions
{
    public class WeeklyNewsletterSender
    {
        private readonly INewsletterService _newsletterService;
        private readonly ILogger<WeeklyNewsletterSender> _logger;

        public WeeklyNewsletterSender(
            INewsletterService newsletterService,
            ILogger<WeeklyNewsletterSender> logger)
        {
            _newsletterService = newsletterService;
            _logger = logger;
        }

        // Runs every Monday at 08:00 UTC
        [Function("WeeklyNewsletterSender")]
        public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo timerInfo) // Change to "0 0 8 * * 1" for production
        {
            _logger.LogInformation("WeeklyNewsletter started at {time}", DateTimeOffset.UtcNow);

            try
            {
                await _newsletterService.SendWeeklyNewsletterAsync(async subscriber =>
                {
                    if (string.IsNullOrWhiteSpace(subscriber.Email))
                    {
                        _logger.LogWarning("Skipping subscriber {UserId}: missing email", subscriber.RowKey);
                        return string.Empty;
                    }

                    return $@"
<html>
    <body>
        <h2>Hello {subscriber.Email},</h2>
        <p>This is a simple test newsletter message.</p>
        <p>We’ll replace this with real content later.</p>
    </body>
</html>";
                });

                _logger.LogInformation("WeeklyNewsletter completed successfully at {time}", DateTimeOffset.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during newsletter dispatch.");
            }
        }
    }
}

