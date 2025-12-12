
using AFHP_NewsSite.Models;
using AFHP_NewsSite.ViewModels.Employee;

namespace AFHP_NewsSite.Services.Newsletter
{
    public interface INewsletterTemplateService
    {
        // General digest builder (delegates internally based on tier)
        NewsletterDigestResult BuildWeeklyDigest(
            NewsletterSubscriptionEntity subscription,
            UserDetailsVM? userDetails,
            List<Article> mostViewed,
            List<Article> mostLiked);

        // Tier-specific digest builders
        NewsletterDigestResult BuildBasicDigest(
            NewsletterSubscriptionEntity subscription,
            List<Article> articles);

        NewsletterDigestResult BuildPremiumDigest(
            NewsletterSubscriptionEntity subscription,
            UserDetailsVM? userDetails,
            List<Article> articles);

        NewsletterDigestResult BuildPremiumPlusDigest(
            NewsletterSubscriptionEntity subscription,
            UserDetailsVM? userDetails,
            List<Article> articles);
    }
}
