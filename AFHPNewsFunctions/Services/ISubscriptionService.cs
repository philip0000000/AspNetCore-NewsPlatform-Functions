using AFHPNewsFunctions.Models;

namespace AFHPNewsFunctions.Services
{
    public interface ISubscriptionService
    {
        IQueryable<Subscription> GetAllActive();

        Task UpdateAsync(Subscription sub);
    }
}
