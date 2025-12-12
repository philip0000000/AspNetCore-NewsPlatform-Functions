using AFHP_NewsSite.Enums;
using AFHP_NewsSite.Models;
using AFHP_NewsSite.ViewModels.Employee;
using System.Linq;

namespace AFHP_NewsSite.Services.Newsletter
{
    public class NewsletterTemplateService : INewsletterTemplateService
    {
        // ✅ Unified weekly digest builder: delegates to tier-specific methods
        public NewsletterDigestResult BuildWeeklyDigest(
    NewsletterSubscriptionEntity subscription,
    UserDetailsVM? userDetails,
    List<Article> mostViewed,
    List<Article> mostLiked)
        {
            return subscription.Tier switch
            {
                SubscriptionTier.Basic => BuildBasicDigest(subscription, mostViewed),
                SubscriptionTier.Premium => BuildPremiumDigest(subscription, userDetails, mostViewed),
                SubscriptionTier.PremiumPlus => BuildPremiumPlusDigest(subscription, userDetails, mostViewed),
                _ => BuildBasicDigest(subscription, mostViewed)
            };
        }

        // ✅ Basic tier: generic digest
        public NewsletterDigestResult BuildBasicDigest(
            NewsletterSubscriptionEntity subscription,
            List<Article> articles)
        {
            var header = "<h2 style='color:#0078d4; text-align:center;'>📰 Weekly Digest</h2>";
            var articleHtml = BuildArticleList(articles.Where(a => a.PublishedTime != null).Take(5));

            return new NewsletterDigestResult
            {
                HtmlContent = $"{header}{articleHtml}{Signature()}",
                Tier = SubscriptionTier.Basic,
                Category = subscription.Category
            };
        }

        // ✅ Premium tier: personalized greeting + curated articles
        public NewsletterDigestResult BuildPremiumDigest(
            NewsletterSubscriptionEntity subscription,
            UserDetailsVM? userDetails,
            List<Article> articles)
        {
            var header = "<h2 style='color:#0078d4; text-align:center;'>🌟 Personalized Digest</h2>";
            var greeting = BuildGreeting(userDetails);

            var articleHtml = BuildArticleList(articles.OrderByDescending(a => a.PublishedTime).Take(5));

            var premiumBlock = @"<div style='margin-top:20px; padding:15px; background-color:#f9f9f9; border-left:4px solid #0078d4;'>
                                    <h3 style='color:#0078d4;'>🌟 Premium Insights</h3>
                                    <p>Exclusive analysis and extended coverage for Premium members.</p>
                                 </div>";

            return new NewsletterDigestResult
            {
                HtmlContent = $"{header}{greeting}{premiumBlock}{articleHtml}{Signature()}",
                Tier = SubscriptionTier.Premium,
                Category = subscription.Category
            };
        }

        // ✅ Premium Plus tier: advanced personalization with preferences
        public NewsletterDigestResult BuildPremiumPlusDigest(
     NewsletterSubscriptionEntity subscription,
     UserDetailsVM? userDetails,
     List<Article> articles)
        {
            var header = "<h2 style='color:#0078d4; text-align:center;'>💎 Premium Plus Digest</h2>";
            var greeting = BuildGreeting(userDetails);

            var categoryEnum = subscription.Category;
            var categoryName = categoryEnum.ToString();

            // Normalize helper
            static string Norm(string? s) =>
                string.IsNullOrWhiteSpace(s) ? string.Empty : s.Trim().ToLowerInvariant();

            // Try to filter by category name
            var filteredArticles = articles
                .Where(a => a.Category != null &&
                            Norm(a.Category.Name) == Norm(categoryName))
                .Take(5)
                .ToList();

            // Fallback if none found
            if (!filteredArticles.Any())
            {
                filteredArticles = articles
                    .OrderByDescending(a => a.PublishedTime)
                    .Take(5)
                    .ToList();
            }

            var articleHtml = BuildArticleList(filteredArticles);

            var premiumPlusBlock = $@"
        <div style='margin-top:20px; padding:15px; background-color:#fffbe6; border-left:4px solid #ff9800;'>
            <h3 style='color:#ff9800;'>💎 Category Highlights</h3>
            <p>You’re receiving tailored content from your selected category: 
               <strong>{categoryName}</strong>.
            </p>
        </div>";

            return new NewsletterDigestResult
            {
                HtmlContent = $"{header}{greeting}{premiumPlusBlock}{articleHtml}{Signature()}",
                Tier = SubscriptionTier.PremiumPlus,
                Category = categoryEnum
            };
        }

        // 🔧 Helper: build greeting
        private string BuildGreeting(UserDetailsVM? userDetails) =>
            userDetails != null
                ? $"<p>Hi <strong>{userDetails.FirstName ?? userDetails.UserName}</strong>,</p>"
                : "<p>Hello Reader,</p>";

        // 🔧 Helper: build article list
        private string BuildArticleList(IEnumerable<Article> articles)
        {
            if (articles == null || !articles.Any()) return "<p>No articles available this week.</p>";

            var listItems = string.Join("", articles.Select(a =>
                $"<li><strong>{a.Headline}</strong><br/>{a.ContentSummary}</li>"));

            return $"<ul>{listItems}</ul>";
        }

        // 🔧 Helper: signature
        private string Signature() =>
            "<p style='margin-top:20px;'>Warm regards,<br/>AFHP News Team</p>";
    }
}
