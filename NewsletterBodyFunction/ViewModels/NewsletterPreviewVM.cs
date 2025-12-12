//using AFHP_NewsSite.Models;

//namespace AFHP_NewsSite.ViewModels
//{
//    public class NewsletterPreviewVM
//    {
//        public string Tier { get; set; }= "Basic";
//        public string Category { get; set; } = "Local";
//        public string? FirstName { get; set; }
//        public string? UserName { get; set; }
//        public string HtmlContent { get; set; } = string.Empty;

//        public List<Article> MostViewedArticles { get; set; } = new();
//        public List<Article> MostLikedArticles { get; set; } = new();
//    }


//}
using AFHP_NewsSite.Enums;
using AFHP_NewsSite.Models;
using System.Collections.Generic;

namespace AFHP_NewsSite.ViewModels
{
    public class NewsletterPreviewVM
    {
        // Personalization
        public SubscriptionTier Tier { get; set; } = SubscriptionTier.Basic;
        public NewsletterCategory Category { get; set; } = NewsletterCategory.Local;

        public string? FirstName { get; set; }
        public string? UserName { get; set; }

        // Rendered HTML preview
        public string HtmlContent { get; set; } = string.Empty;

        // Article collections
        public List<Article> MostViewedArticles { get; set; } = new();
        public List<Article> MostLikedArticles { get; set; } = new();

        // Optional: Editor’s Choice or Recommended
        public Article? EditorsChoice { get; set; }
        public List<Article> RecommendedArticles { get; set; } = new();
    }
}


