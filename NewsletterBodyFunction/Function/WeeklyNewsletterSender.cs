using AFHP_NewsSite.Enums;
using AFHP_NewsSite.Models;
using AFHP_NewsSite.Services;
using AFHP_NewsSite.Services.Claims_Roles;
using AFHP_NewsSite.Services.Newsletter;
using AFHP_NewsSite.ViewModels.Employee;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace NewsletterFunctionApp.Functions
{
    public class WeeklyNewsletterSender
    {
        private readonly INewsletterService _newsletterService;
        private readonly IAnalyticsService _analyticsService;
        private readonly IUserService _userService;
        private readonly INewsletterTemplateService _templateService;
        private readonly ILogger<WeeklyNewsletterSender> _logger;

        public WeeklyNewsletterSender(
            INewsletterService newsletterService,
            IAnalyticsService analyticsService,
            IUserService userService,
            INewsletterTemplateService templateService,
            ILogger<WeeklyNewsletterSender> logger)
        {
            _newsletterService = newsletterService;
            _analyticsService = analyticsService;
            _userService = userService;
            _templateService = templateService;
            _logger = logger;
        }

        // Runs every Monday at 08:00 UTC
        [Function("WeeklyNewsletterSender")]
        public async Task Run([TimerTrigger("0 0 8 * * 1")] TimerInfo timerInfo) 
        {
            _logger.LogInformation("WeeklyNewsletter started at {time}", DateTimeOffset.UtcNow);

            try
            {
                // Preload analytics data once per run
                var mostViewed = await _analyticsService.GetMostViewedArticlesAsync();
                var mostLiked = await _analyticsService.GetMostLikedArticlesAsync();

                await _newsletterService.SendWeeklyNewsletterAsync(async subscriber =>
                {
                    if (string.IsNullOrWhiteSpace(subscriber.Email))
                    {
                        _logger.LogWarning("Skipping subscriber {UserId}: missing email", subscriber.RowKey);
                        return new NewsletterDigestResult
                        {
                            HtmlContent = string.Empty,
                            PlainTextContent = string.Empty,
                            Tier = SubscriptionTier.Basic,
                            Category = NewsletterCategory.Local
                        };
                    }

                    // Load personalization data
                    var userDetails = await _userService.GetUserDetailsAsync(subscriber.RowKey);

                    // Build tier‑specific digest
                    var digest = _templateService.BuildWeeklyDigest(
                        subscriber,
                        userDetails,
                        mostViewed,
                        mostLiked
                    );

                    _logger.LogInformation("Prepared digest for {Email} with Tier={Tier}, Category={Category}",
                        subscriber.Email, digest.Tier, digest.Category);

                    return digest;
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
