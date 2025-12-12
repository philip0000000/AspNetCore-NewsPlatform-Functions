using AFHP_NewsSite.Services.Claims_Roles;
using AFHP_NewsSite.ViewModels;
using AFHP_NewsSite.ViewModels.Employee;

public class UserService : IUserService
{
    /// <summary>
    /// Returns fake user details for testing newsletter flow.
    /// </summary>
    public Task<UserDetailsVM?> GetUserDetailsAsync(string userId)
    {
        // For testing, just return a dummy user with predictable values
        var user = new UserDetailsVM
        {
            Id = userId,
            UserName = $"testuser_{userId}",
            Email = $"{userId}@example.com",
            FirstName = "Test",
            LastName = "User",
            //DOB = DateTime.UtcNow.AddYears(-25),
            RegistrationDate = DateTime.UtcNow.AddMonths(-6),
            IsActive = true,
            EmailConfirmed = true,
            Roles = new List<string> { "Tester" },
            Claims = new List<string> { "Newsletter:Subscribed" },
            //LatestSubscription = null,
            HasActiveSubscription = true,
            //TotalMonthsSubscribed = 6,
            //TotalSubscriptionRevenue = 29.99m
        };

        return Task.FromResult<UserDetailsVM?>(user);
    }
}
