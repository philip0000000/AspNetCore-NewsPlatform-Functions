using AFHPNewsFunctions.Models;
using AFHPNewsFunctions.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AFHPNewsFunctions.Functions;

public class SubscriptionMaintenanceFunction
{
    private const int DAYSGRACEPERIOD = 3;

    private readonly ILogger _logger;
    private readonly ISubscriptionService _subscriptionService;
    private readonly UserManager<ApplicationUser> _userManager;

    public SubscriptionMaintenanceFunction(
        ILoggerFactory loggerFactory,
        ISubscriptionService subscriptionService,
        UserManager<ApplicationUser> userManager)
    {
        _logger = loggerFactory.CreateLogger<SubscriptionMaintenanceFunction>();
        _subscriptionService = subscriptionService;
        _userManager = userManager;
    }

    // NOTE: Might be more effective to just look at those subs where date >= the last time this function was called, if possible
    //  Use Transactions?

    // 1. Will check paid subs where sub.Payment.NextBilling < Today
    //   and sync sub.IsPaid & sub.Payment.NextBilling with Payment Processor (simulated), and update sub.Expires, sub.Payment.PaidUntil
    // 2. Will check for unpaid subs and set sub.Expires to Today if beyond Grace period
    // 3. Will check all active subs and if sub.Expires <= Today it will remove the "Subscription" Claim from the User and inactivate the sub
    [Function("SubscriptionMaintenance")]
    public async Task Run([TimerTrigger("0 0 2 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("Timer trigger SubscriptionMaintenance Function executed at: {executionTime}", DateTime.Now);

        try
        {
            await SyncPaidSubscriptions();
            await HandleUnpaidSubscriptions();
            await RemoveSubscriptionClaims();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SubscriptionMaintenance Function");

            // Try send email to admin?
            // await _emailSender.SendEmailAsync("admin.afhp.news@gmail.com", ...);

            // Re-throw so Azure marks the run as failed?
            // throw;
        }

        if (myTimer.ScheduleStatus is not null)
            _logger.LogInformation("SubscriptionMaintenance Function Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
    }

    // 1. Will check paid subs where sub.Payment.NextBilling < Today
    //   and sync sub.IsPaid & sub.Payment.NextBilling with Payment Processor, and update sub.Expires, sub.Payment.PaidUntil
    private async Task SyncPaidSubscriptions()
    {
        var today = DateTime.Today;
        var subs = await _subscriptionService.GetAllActive()
            .Where(s => s.IsPaid && s.Payment.NextBillingUtc.HasValue &&
                s.Payment.NextBillingUtc.Value < today)
            .ToListAsync();

        foreach (var sub in subs)
        {
            var user = sub.User;

            // TODO: Check with real Payment Processor if subscription is paid,
            //  Let's just simulate this for now, that it always is true, could simulate that it fails 5-10% of the cases?
            //  If it is not paid: set sub.Payment.IsPaid = false

            var nextBilling = sub.Payment.PaidUntilUtc.AddDays(30); // Just add 30 days
            sub.Payment.PaidUntilUtc = nextBilling;
            sub.Payment.NextBillingUtc = nextBilling;
            sub.Expires = sub.Payment.PaidUntilUtc.AddDays(DAYSGRACEPERIOD); // Add Grace Period

            await _subscriptionService.UpdateAsync(sub);

            _logger.LogInformation("Updated Paid {} Subscription {}: {}", sub.SubscriptionType.TypeName, user.Email, nextBilling.ToString("yyyy-MM-dd"));
        }
    }

    // 2. Set sub.Expires to Today if Unpaid Subscription is beyond Grace period
    private async Task HandleUnpaidSubscriptions()
    {
        var today = DateTime.Today;
        var subs = await _subscriptionService.GetAllActive()
            .Where(s => !s.IsPaid && s.Payment.NextBillingUtc.HasValue &&
                s.Payment.NextBillingUtc.Value.AddDays(DAYSGRACEPERIOD) < today)
            .ToListAsync();

        foreach (var sub in subs)
        {
            var user = sub.User;

            sub.Expires = today;
            sub.Payment.NextBillingUtc = null;

            await _subscriptionService.UpdateAsync(sub);

            _logger.LogInformation("Cancelled Unpaid {} Subscription {}", sub.SubscriptionType.TypeName, user.Email);
        }
    }

    // 3. Remove Claim and inactivate expired Subscriptions
    private async Task RemoveSubscriptionClaims()
    {
        var today = DateTime.Today;
        var subs =  await _subscriptionService.GetAllActive()
            .Where(s => s.Expires <= today)
            .ToListAsync();

        foreach (var sub in subs)
        {
            var subType = sub.SubscriptionType.TypeName;
            var user = sub.User;
            var claims = await _userManager.GetClaimsAsync(user);

            var inactivate = true;
            var subscriptionClaim = claims.FirstOrDefault(c => c.Type == "Subscription");
            if (subscriptionClaim != null && subscriptionClaim.Value == subType)
            {
                var claimResult = await _userManager.RemoveClaimAsync(user, subscriptionClaim);
                if (claimResult.Succeeded)
                    _logger.LogInformation("Removed {} Subscription Claim from User {}", subType, user.Email);
                else
                {
                    _logger.LogInformation("Failed to remove {} Subscription Claim from User {}", subType, user.Email);
                    inactivate = false;
                }
            }

            if (inactivate)
            {
                sub.IsActive = false;
                sub.Payment.NextBillingUtc = null;

                await _subscriptionService.UpdateAsync(sub);

                _logger.LogInformation("Inactivated {} Subscription {}", subType, user.Email);
            }
        }
    }
}