using AFHP_NewsSite.Models;
using AFHP_NewsSite.ViewModels;

namespace AFHP_NewsSite.ViewModels
{
    public class AnalyticsViewModel
    {
        public int ArticleViews {  get; set; }
        public int ArticleLikes { get; set; }
        public int ArticlesPublished { get; set; }
        public List<Article> MostViewedArticles { get; set; } = new List<Article>();
        public List<Article> MostLikedArticles { get; set; } = new List<Article>();
        public List<Article> RecentlyPublishedArticles { get; set; } = new List<Article>();
        public List<TopWriterVM> TopWriters { get; set; } = new List<TopWriterVM>();
        public SubscriptionStatsVM ArticlePerAuthor { get; set; } = new SubscriptionStatsVM();
        public SubscriptionStatsVM ArticlePerCategory { get; set; } = new SubscriptionStatsVM();
        public int PendingArticlesTotal { get; set; }
        public int PublishedArticlesTotal { get; set; }
        public int DraftArticlesTotal { get; set; }
    }
}

