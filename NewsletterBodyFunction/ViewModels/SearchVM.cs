using AFHP_NewsSite.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AFHP_NewsSite.ViewModels
{
    public class SearchVM
    {
        public string? SearchString { get; set; }
        public bool? SearchHeadlines { get; set; }
        public bool? SearchSummary { get; set; }
        public bool? SearchContent { get; set; }
        public bool? SearchArchived { get; set; }
        public int ArticlesMaxCount { get; set; } = 100;
        public IEnumerable<Article> Articles { get; set; } = new List<Article>();
        public List<SelectListItem> CategoriesSelectList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> DateRangeSelectList { get; set; } = new List<SelectListItem>();

    }
}
