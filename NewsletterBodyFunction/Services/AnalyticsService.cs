using AFHP_NewsSite.Models;
using AFHP_NewsSite.ViewModels;
using AFHPNewsFunctions.Data;
using Microsoft.EntityFrameworkCore;

namespace AFHP_NewsSite.Services
{
    public class AnalyticsService: IAnalyticsService
    {
        private readonly NewsletterDbContext _context;

        public AnalyticsService(NewsletterDbContext context)
        {
            _context = context;
        }
        public async Task<List<Article>> GetMostViewedArticlesAsync() 
        {
            var articles = await _context.Articles
                .OrderByDescending(a => a.Views)
                .Take(5)
                .ToListAsync();
            return articles;
        }

        public async Task<List<Article>> GetMostLikedArticlesAsync()
        {
            var articles = await _context.Articles
                .OrderByDescending(a => a.Likes)
                .Take(5)
                .ToListAsync();
            return articles;
        }

        public async Task<List<Article>> GetRecentlyPublishedArticlesAsync()
        {
            var articles = await _context.Articles
                .OrderByDescending(a=>a.PublishedTime)
                .Take(5)
                .ToListAsync();
            return articles;
        }

        public async Task<List<TopWriterVM>> GetTopWriters()
        {
            var topWriters = await _context.Articles
                .GroupBy(a => new { a.Author.FirstName, a.Author.LastName })
                .Select(g => new TopWriterVM
                {
                    AuthorFirstName = g.Key.FirstName,
                    AuthorLastName = g.Key.LastName,
                    ArticleCount = g.Count()
                })
                .OrderByDescending(x => x.ArticleCount)
                .Take(10)
                .ToListAsync();
            return topWriters;
        }

        public async Task<int> GetTotalPublishedArticlesAsync()
        {
            var count = await _context.Articles
                    .Where(a => a.ArticleStatus.StatusName == "Published")
                    .CountAsync();
            return count;
        }

        public async Task<int> GetTotalViewsAsync()
        {
            var count = await _context.Articles
                .SumAsync(a => a.Views);
            return count;
        }

        public async Task<int> GetTotalLikesAsync()
        {
            var count = await _context.Articles
                .SumAsync(a => a.Likes);
            return count;
        }

        public async Task<SubscriptionStatsVM> GetArticlesPerAuthorAsync()
        {
            var result = await _context.Articles
                .Include(a => a.Author)
                .GroupBy(a => new { a.Author.FirstName, a.Author.LastName })
                .Select(g => new 
                {
                    AuthorName = g.Key.FirstName + " " + g.Key.LastName,
                    ArticleCount = g.Count()
                })
                .ToListAsync();

            var viewModel = new SubscriptionStatsVM();
            foreach (var item in result)
            {
                viewModel.Labels.Add(item.AuthorName);
                viewModel.Data.Add(item.ArticleCount);
            }
            return viewModel;
        }

        public async Task<SubscriptionStatsVM> GetArticlesPerCategoryAsync()
        {
            var result = await _context.Articles
                .Include(a => a.Category)
                .GroupBy(a => new { a.Category.Name })
                .Select(g => new
                {
                    CategoryName = g.Key.Name,
                    CateoryCount = g.Count()
                })
                .ToListAsync();

            var viewModel = new SubscriptionStatsVM();
            foreach (var item in result)
            {
                viewModel.Labels.Add(item.CategoryName);
                viewModel.Data.Add(item.CateoryCount);
            }
            return viewModel;
        }

        public async Task<int> GetPendingArticlesTotal()
        {
            var count = await _context.Articles
                .Include(a => a.ArticleStatus)
                .Where(a => a.ArticleStatus.StatusName == "Review")
                .CountAsync();
            return count;
        }

        public async Task<int> GetPublishedArticlesTotal()
        {
            var count = await _context.Articles
                .Include(a => a.ArticleStatus)
                .Where(a => a.ArticleStatus.StatusName == "Published")
                .CountAsync();
            return count;
        }
        public async Task<int> GetDraftArticlesTotal()
        {
            var count = await _context.Articles
                .Include(a => a.ArticleStatus)
                .Where(a => a.ArticleStatus.StatusName == "Draft")
                .CountAsync();
            return count;
        }

    }
}
