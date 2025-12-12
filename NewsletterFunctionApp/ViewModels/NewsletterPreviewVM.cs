

namespace AFHP_NewsSite.ViewModels
{
    public class NewsletterPreviewVM
    {
        public string Tier { get; set; }= "Basic";
        public string Category { get; set; } = "Local";
        public string? FirstName { get; set; }
        public string? UserName { get; set; }
        public string HtmlContent { get; set; } = string.Empty;

    }


}

