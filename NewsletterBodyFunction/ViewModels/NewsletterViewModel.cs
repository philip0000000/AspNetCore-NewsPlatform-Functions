//using AFHP_NewsSite.ViewModels.Employee;

//namespace AFHP_NewsSite.ViewModels
//{
//    public class NewsletterViewModel
//    {       
//        public bool IsSubscribed { get; set; }
//        public UserDetailsVM UserDetails { get; set; } = new UserDetailsVM();
//    }
//}
using AFHP_NewsSite.Enums;
using AFHP_NewsSite.Models;
using AFHP_NewsSite.ViewModels.Employee;

namespace AFHP_NewsSite.ViewModels
{
    public class NewsletterViewModel
    {
        // Subscription state
        public bool IsSubscribed { get; set; }

        // User details
        public UserDetailsVM UserDetails { get; set; } = new();

        // Strongly typed tier and category
        public SubscriptionTier Tier { get; set; } = SubscriptionTier.Basic;
        public NewsletterCategory Category { get; set; } = NewsletterCategory.Local;
    }
}
