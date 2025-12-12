using AFHP_NewsSite.Models;
using AFHP_NewsSite.ViewModels;

namespace AFHP_NewsSite.Services
{
    public interface IAnalyticsService
    {
        Task<List<Article>> GetMostViewedArticlesAsync();
        Task<List<Article>> GetMostLikedArticlesAsync();
        Task<List<Article>> GetRecentlyPublishedArticlesAsync();
        Task<List<TopWriterVM>> GetTopWriters();
        Task<int> GetTotalPublishedArticlesAsync();
        Task<int> GetTotalViewsAsync();
        Task<int> GetTotalLikesAsync();
        Task<SubscriptionStatsVM> GetArticlesPerAuthorAsync();
        Task<SubscriptionStatsVM> GetArticlesPerCategoryAsync();
        Task<int> GetPendingArticlesTotal();
        Task<int> GetPublishedArticlesTotal();
        Task<int> GetDraftArticlesTotal();

    }
}
