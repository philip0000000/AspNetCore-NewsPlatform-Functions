
using AFHP_NewsSite.ViewModels;
using AFHP_NewsSite.ViewModels.Employee;


namespace AFHP_NewsSite.Services.Claims_Roles
{
    public interface IUserService
    {
        Task<UserDetailsVM?> GetUserDetailsAsync(string userId);
    }
}


