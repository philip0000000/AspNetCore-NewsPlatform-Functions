using AFHP_NewsSite.ViewModels.Employee;

namespace AFHP_NewsSite.ViewModels
{
    public class NewsletterViewModel
    {       
        public bool IsSubscribed { get; set; }
        public UserDetailsVM UserDetails { get; set; } = new UserDetailsVM();
    }
}
