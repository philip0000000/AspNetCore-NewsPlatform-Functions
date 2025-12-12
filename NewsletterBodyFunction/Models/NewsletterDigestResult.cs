using AFHP_NewsSite.Enums;
using AFHP_NewsSite.Models;

namespace AFHP_NewsSite.Models
{
    public class NewsletterDigestResult
    {
        public string HtmlContent { get; set; } = string.Empty;
        public string? PlainTextContent { get; set; }
        public SubscriptionTier Tier { get; set; }
        public NewsletterCategory Category { get; set; }
    }

}

