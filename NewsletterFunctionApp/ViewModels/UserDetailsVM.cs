namespace AFHP_NewsSite.ViewModels.Employee
{
    
    public class UserDetailsVM
    {
        public string Id { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string UserName { get; init; } = string.Empty;

        public string FirstName { get; init; } = string.Empty;

        public string LastName { get; init; } = string.Empty;

        public DateOnly? DOB { get; init; }

        public DateTime RegistrationDate { get; init; }

        public bool IsActive { get; init; }

        public bool EmailConfirmed { get; init; }

        public bool HasActiveSubscription { get; init; }

        public List<string> Roles { get; init; } = new();

        //e.g.,["Permission:CreateArticle","Permission:EditArticle"]
        public List<string> Claims{ get; init; } = new();

        //public Models.Subscription? LatestSubscription { get; set; }
        //public int TotalMonthsSubscribed { get; init; }
        //public decimal TotalSubscriptionRevenue { get; init; }
    }

}
