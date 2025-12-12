namespace AFHP_NewsSite.ViewModels.Articles
{
    public class ArticleListFilterVM
    {
        public string? Search { get; set; }
        public string? SelectedCategory { get; set; }
        public string? SelectedStatus { get; set; }
        public int PageSize { get; set; } = 10;
    }
}
