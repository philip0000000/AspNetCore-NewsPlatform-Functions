using AFHPNewsFunctions.Models;
using AFHPNewsFunctions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AFHPNewsFunctions.Functions;

// NOTE: The datetimes in the DB are UTC, and we don't know which timezone the receiver is in,
// so we need to specify both date and time UTC in the email.

public class SubscriptionEmailsFunction
{
    private readonly ILogger _logger;
    private readonly IEmailSender _emailSender;
    private readonly ISubscriptionService _subscriptionService;

    private const int DAYSUNTILNEXTBILLING = 2;
    private const int DAYSUNTILSUBEXPIRING = 2;
    private const int DAYSAFTERNEXTBILLING = 2;
    private const int DAYSGRACEPERIOD = 3;

    // 2-3 seconds between sending emails
    private const int MINDELAYSECONDS = 2;
    private const int MAXDELAYSECONDS = 3;

    public SubscriptionEmailsFunction(
        ILoggerFactory loggerFactory,
        IEmailSender emailSender,
        ISubscriptionService subscriptionService)
    {
        _logger = loggerFactory.CreateLogger<SubscriptionEmailsFunction>();
        _emailSender = emailSender;
        _subscriptionService = subscriptionService;
    }

    // 1. Will send email DAYSUNTILNEXTBILLING days before a subscription will be billed
    // 2. Will send email DAYSUNTILSUBEXPIRING days before a cancelled subscription ends (while it is still possible to renew)
    // 3. Will send email DAYSAFTERNEXTBILLING days after billing date, if not paid yet

    
    // Daily at 3am
    // NOTE: Remove RunOnStartup before deploying
    // TimerTrigger("0 0 3 * * *", RunOnStartup = true)
    [Function("SubscriptionEmails")]
    public async Task Run([TimerTrigger("0 0 3 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("Timer trigger SubscriptionEmails Function started at: {executionTime}", DateTime.Now);

        try
        {
            // In Serial
            await SendBillingReminder();
            await SendSubscriptionExpiryReminder();
            await SendFailedBillingReminder();

            // In parallel
            //await Task.WhenAll(
            //    SendBillingReminder(subs),
            //    SendSubscriptionExpiryReminder(subs),
            //    SendFailedBillingReminder(subs)
            //);

            _logger.LogInformation("SubscriptionEmails Function completed successfully at: {executionTime}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SubscriptionEmails Function");

            // Try send email to admin?
            // await _emailSender.SendEmailAsync("admin.afhp.news@gmail.com", ...);

            // Re-throw so Azure marks the run as failed?
            // throw;
        }

        if (myTimer.ScheduleStatus is not null)
            _logger.LogInformation("SubscriptionEmails Function Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
    }

    // Will send email DAYSUNTILNEXTBILLING days before a subscription will be billed
    private async Task SendBillingReminder()
    {
        var targetDate = DateTime.UtcNow.AddDays(DAYSUNTILNEXTBILLING).Date;
        var nextDate = targetDate.AddDays(1);

        var subs = await _subscriptionService.GetAllActive()
            .Where(s =>
                s.IsPaid &&
                s.Payment.NextBillingUtc.HasValue &&
                s.Payment.NextBillingUtc.Value >= targetDate &&
                s.Payment.NextBillingUtc.Value < nextDate
            )
            .ToListAsync();

        foreach (var sub in subs)
        {
            var email = sub.User.Email!;
            var name = $"{sub.User.FirstName} {sub.User.LastName}";
            var billingDate = $"{sub.Payment.NextBillingUtc:yyyy-MM-dd HH:mm} UTC";
            var price = sub.Price.ToString();
            var type = sub.SubscriptionType.TypeName;
            var success = await SendBillingReminderEmail(email, name, billingDate, price, type);
        }
    }

    private async Task<bool> SendBillingReminderEmail(string email, string name, string billingDate, string price, string type)
    {
        var subject = "AFHP News - Billing Reminder";

        var htmlMessage = $@"
        <div style='text-align:center; margin-bottom:20px;'>
            <a href='https://afhpnewssite.azurewebsites.net/'>
                <img src='https://afhpnewssite.azurewebsites.net/context/images/screenshot%202025-10-15%20103611.png' 
                     alt='AFHP News Logo' 
                     style='max-width:150px; height:auto;' />
            </a>
        </div>

        <h2 style='color:#0078d4; text-align:center;'>Billing Reminder</h2>
        <p style='font-family:Segoe UI, Arial, sans-serif; font-size:14px; color:#333;'>
            Hi <strong>{name}</strong>,
        </p>
        <p style='font-family:Segoe UI, Arial, sans-serif; font-size:14px; color:#333;'>
            Thank you for subscribing to AFHP News! <br><br>
            This is a reminder that on <strong>{billingDate}</strong>, you will be billed <strong>{price}</strong> for the next period of your <strong>{type}</strong> subscription.<br><br>
            If you wish to continue your subscription, no further action is required.
        </p>

        <p style='margin-top:20px;'><strong><em>Best regards,<br/>The AFHP News Team</em></strong></p>
        ";

        return await SendEmailAsync(email, subject, htmlMessage);
    }

    // Will send email DAYSUNTILSUBEXPIRING days before a cancelled subscription ends (while it is still possible to renew)
    private async Task SendSubscriptionExpiryReminder()
    {
        var targetDate = DateTime.UtcNow.AddDays(DAYSUNTILSUBEXPIRING).Date;
        var nextDate = targetDate.AddDays(1);

        var subs = await _subscriptionService.GetAllActive()
            .Where(s =>
                s.IsPaid && s.Payment.NextBillingUtc == null &&
                s.Payment.PaidUntilUtc >= targetDate &&
                s.Payment.PaidUntilUtc < nextDate
            )
            .ToListAsync();

        foreach (var sub in subs)
        {
            var email = sub.User.Email!;
            var name = $"{sub.User.FirstName} {sub.User.LastName}";
            var expiryDate = $"{sub.Payment.PaidUntilUtc:yyyy-MM-dd HH:mm} UTC";
            var type = sub.SubscriptionType.TypeName;
            var success = await SendSubscriptionExpiryReminderEmail(email, name, expiryDate, type);
        }
    }
    private async Task<bool> SendSubscriptionExpiryReminderEmail(string email, string name, string expiryDate, string type)
    {
        var subject = "AFHP News - Your subscription is ending soon";

        var htmlMessage = $@"
        <div style='text-align:center; margin-bottom:20px;'>
            <a href='https://afhpnewssite.azurewebsites.net/'>
                <img src='https://afhpnewssite.azurewebsites.net/context/images/screenshot%202025-10-15%20103611.png' 
                     alt='AFHP News Logo' 
                     style='max-width:150px; height:auto;' />
            </a>
        </div>

        <h2 style='color:#0078d4; text-align:center;'>Subscription is ending soon</h2>
        <p style='font-family:Segoe UI, Arial, sans-serif; font-size:14px; color:#333;'>
            Hi <strong>{name}</strong>,
        </p>
        <p style='font-family:Segoe UI, Arial, sans-serif; font-size:14px; color:#333;'>
            Thank you for being a subscriber to AFHP News! <br><br>
            This is a reminder that your <strong>{type}</strong> subscription will expire on <strong>{expiryDate}</strong>.<br>
            To continue enjoying our content without interruption, you can renew your subscription before that date. If it has already expired, you can easily start a new one anytime.
        </p>

        <p style='margin-top:20px;'><strong><em>Best regards,<br/>The AFHP News Team</em></strong></p>
        ";

        return await SendEmailAsync(email, subject, htmlMessage);
    }

    // Will send email DAYSAFTERNEXTBILLING days after billing date, if not paid yet
    // Should this also check Created date, if first payment failed?
    private async Task SendFailedBillingReminder()
    {
        //var subsData = await _subscriptionService.GetAllActive()
        //    .Where(s => !s.IsPaid && s.Payment.NextBillingUtc.HasValue) // Not Paid
        //    .Select(s => new
        //    {
        //        sub = s,
        //        days = (today - s.Payment.NextBillingUtc!.Value.Date).Days // Days since billing
        //    })
        //    .Where(r => r.days > 0 && r.days <= DAYSAFTERNEXTBILLING)
        //    .ToListAsync();

        var today = DateTime.UtcNow.Date;
        var startDate = today.AddDays(-DAYSAFTERNEXTBILLING);
        var endDate = today.AddDays(-1);

        var subs = await _subscriptionService.GetAllActive()
            .Where(s => !s.IsPaid
                && s.Payment.NextBillingUtc.HasValue
                && s.Payment.NextBillingUtc.Value.Date >= startDate
                && s.Payment.NextBillingUtc.Value.Date <= endDate)
            .ToListAsync();

        foreach (var sub in subs)
        {
            var email = sub.User.Email!;
            var name = $"{sub.User.FirstName} {sub.User.LastName}";
            var gracePeriodEndDate = $"{sub.Payment.NextBillingUtc?.AddDays(DAYSGRACEPERIOD):yyyy-MM-dd HH:mm} UTC";
            var price = sub.Price.ToString();
            var type = sub.SubscriptionType.TypeName;
            var success = await SendFailedBillingEmail(email, name, gracePeriodEndDate, price, type);
        }
    }

    private async Task<bool> SendFailedBillingEmail(string email, string name, string gracePeriodEndDate, string price, string type)
    {
        var subject = "AFHP News – Payment Issue";

        var htmlMessage = $@"
        <div style='text-align:center; margin-bottom:20px;'>
            <a href='https://afhpnewssite.azurewebsites.net/'>
                <img src='https://afhpnewssite.azurewebsites.net/context/images/screenshot%202025-10-15%20103611.png' 
                     alt='AFHP News Logo' 
                     style='max-width:150px; height:auto;' />
            </a>
        </div>

        <h2 style='color:#0078d4; text-align:center;'>Payment Issue</h2>
        <p style='font-family:Segoe UI, Arial, sans-serif; font-size:14px; color:#333;'>
            Hi <strong>{name}</strong>,
        </p>
        <p style='font-family:Segoe UI, Arial, sans-serif; font-size:14px; color:#333;'>
            Thank you for being a subscriber to AFHP News! <br><br>
            We attempted to bill <strong>{price}</strong> for your monthly <strong>{type}</strong> subscription, but the payment was declined by the processor.<br>
            To avoid any interruption in your access, please update your payment method before <strong>{gracePeriodEndDate}</strong>. If your subscription has already expired, you can easily start a new one at any time.
        </p>

        <p style='margin-top:20px;'><strong><em>Best regards,<br/>The AFHP News Team</em></strong></p>
        ";

        return await SendEmailAsync(email, subject, htmlMessage);
    }

    // Free Gmail: 500 emails/ day, 2–3 seconds between emails
    private async Task<bool> SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            var response = await _emailSender.SendEmailWithDelayAsync(email, subject, htmlMessage, MINDELAYSECONDS*1000, MAXDELAYSECONDS*1000);
            if (response == null)
            {
                _logger.LogInformation("{subject} Email sent to {email}", subject, email);

                return true;
            }

            _logger.LogError("Sending {Subject} to {Email} failed.", subject, email);
            _logger.LogError(response);

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sending {Subject} to {Email} failed.", subject, email);

            return false;
        }
    }
}