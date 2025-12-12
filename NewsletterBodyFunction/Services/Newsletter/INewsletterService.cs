using AFHP_NewsSite.Enums;
using AFHP_NewsSite.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AFHP_NewsSite.Services.Newsletter
{
    public interface INewsletterService
    {
        // ✅ Subscription management
        Task UpdateSubscriptionAsync(string userId, SubscriptionTier tier, NewsletterCategory category);

        Task SubscribeAsync(
            string userId,
            string email,
            SubscriptionTier tier = SubscriptionTier.Basic,
            NewsletterCategory category = NewsletterCategory.Local
        );

        Task UnsubscribeAsync(string userId);

        Task<bool> IsSubscribedAsync(string userId);

        // ✅ Subscriber queries
        Task<IEnumerable<NewsletterSubscriptionEntity>> GetSubscribersAsync(
            SubscriptionTier? tier = null,
            NewsletterCategory? category = null
        );

        // ✅ Confirmation emails
        Task SendConfirmationEmailAsync(
            string email,
            bool subscribed,
            SubscriptionTier tier = SubscriptionTier.Basic
        );

        // ✅ Weekly newsletter delivery (tier‑specific digest)
        Task SendWeeklyNewsletterAsync(Func<NewsletterSubscriptionEntity, Task<NewsletterDigestResult>> digestBuilder);
    }
}
