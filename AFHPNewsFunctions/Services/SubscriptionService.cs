using AFHPNewsFunctions.Data;
using AFHPNewsFunctions.Models;
using Microsoft.EntityFrameworkCore;

namespace AFHPNewsFunctions.Services
{
    internal class SubscriptionService :ISubscriptionService
    {
        private readonly ApplicationDbContext _context;
        public SubscriptionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Subscription> GetAllActive()
        {
            var subs = _context.Subscriptions
            .Include(s => s.Payment)
            .Include(s => s.SubscriptionType)
            .Include(s => s.User)
            .Where(s => s.IsActive);

            return subs;
        }

        public async Task UpdateAsync(Subscription sub)
        {
            _context.Subscriptions.Update(sub);
            await _context.SaveChangesAsync();
        }
    }
}
