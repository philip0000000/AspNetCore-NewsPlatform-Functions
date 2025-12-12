using AFHP_NewsSite.Enums;
using AFHP_NewsSite.Models;
using AFHP_NewsSite.Services.Claims_Roles;
using AFHP_NewsSite.Services.Newsletter;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using NewsletterBodyFunction.Services;
using System.Text.RegularExpressions;

public class AzureNewsletterService : INewsletterService
{
    private const string Partition = "Newsletter";

    private readonly TableClient _tableClient;
    private readonly IEmailSender _emailSender;
    private readonly IUserService _userService;
    private readonly ILogger<AzureNewsletterService> _logger;

    public AzureNewsletterService(
    TableClient tableClient,
    IEmailSender emailSender,
    IUserService userService,
    ILogger<AzureNewsletterService> logger)
    {
        _tableClient = tableClient;
        _emailSender = emailSender;
        _userService = userService;
        _logger = logger;
    }


    public async Task UpdateSubscriptionAsync(string userId, SubscriptionTier tier, NewsletterCategory category)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<NewsletterSubscriptionEntity>(Partition, userId);
            var subscription = response.Value;

            subscription.Tier = tier;
            subscription.Category = category;

            await _tableClient.UpdateEntityAsync(subscription, subscription.ETag, TableUpdateMode.Replace);
            _logger.LogInformation("Updated subscription for {UserId} to Tier={Tier}, Category={Category}", userId, tier, category);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            var newEntity = new NewsletterSubscriptionEntity
            {
                PartitionKey = Partition,
                RowKey = userId,
                Tier = tier,
                Category = category,
                IsSubscribed = true,
                SubscribedOn = DateTime.UtcNow
            };

            await _tableClient.UpsertEntityAsync(newEntity);
            _logger.LogInformation("Created subscription for {UserId} Tier={Tier}, Category={Category}", userId, tier, category);
        }
    }

    public async Task SubscribeAsync(string userId, string email, SubscriptionTier tier = SubscriptionTier.Basic, NewsletterCategory category = NewsletterCategory.Local)
    {
        var entity = new NewsletterSubscriptionEntity
        {
            PartitionKey = Partition,
            RowKey = userId,
            Email = email,
            IsSubscribed = true,
            Tier = tier,
            Category = category,
            SubscribedOn = DateTime.UtcNow
        };

        await _tableClient.UpsertEntityAsync(entity);
        _logger.LogInformation("Subscribed {UserId} with {Email} Tier={Tier}, Category={Category}", userId, email, tier, category);
    }

    public async Task UnsubscribeAsync(string userId)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<NewsletterSubscriptionEntity>(Partition, userId);
            var entity = response.Value;

            entity.IsSubscribed = false;
            entity.UnsubscribedOn = DateTime.UtcNow;

            await _tableClient.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace);
            _logger.LogInformation("Unsubscribed {UserId}", userId);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Unsubscribe attempted for non-existent {UserId}", userId);
        }
    }

    public async Task<bool> IsSubscribedAsync(string userId)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<NewsletterSubscriptionEntity>(Partition, userId);
            return response.Value.IsSubscribed;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }

    public async Task<IEnumerable<NewsletterSubscriptionEntity>> GetSubscribersAsync(SubscriptionTier? tier = null, NewsletterCategory? category = null)
    {
        var filter = TableClient.CreateQueryFilter<NewsletterSubscriptionEntity>(e => e.PartitionKey == Partition && e.IsSubscribed);
        var results = new List<NewsletterSubscriptionEntity>();

        await foreach (var entity in _tableClient.QueryAsync<NewsletterSubscriptionEntity>(filter))
        {
            var tierMatch = !tier.HasValue || entity.Tier == tier.Value;
            var categoryMatch = !category.HasValue || entity.Category == category.Value;

            if (tierMatch && categoryMatch)
            {
                results.Add(entity);
            }
        }

        return results;
    }

    public async Task SendConfirmationEmailAsync(string email, bool subscribed, SubscriptionTier tier = SubscriptionTier.Basic)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("Confirmation email skipped: missing email");
            return;
        }

        var subject = subscribed ? "Newsletter Subscription Confirmed" : "Newsletter Unsubscribed";
        var tierText = tier == SubscriptionTier.Basic ? string.Empty : $" as a {tier} member";

        var body = subscribed
            ? $"<h2>Welcome aboard!</h2><p>Thank you for joining our newsletter family{tierText}.</p>"
            : "<h2>We’ll miss you!</h2><p>You’ve successfully unsubscribed from our newsletter.</p>";

        var digest = new NewsletterDigestResult
        {
            HtmlContent = body,
            PlainTextContent = Regex.Replace(body, "<.*?>", string.Empty),
            Tier = tier,
            Category = NewsletterCategory.Local,
            //Subject = subject
        };

        await _emailSender.SendNewsletterAsync(email, digest);
        _logger.LogInformation("Confirmation email queued to {Email}: {Subject}", email, subject);
    }

    public async Task SendWeeklyNewsletterAsync(Func<NewsletterSubscriptionEntity, Task<NewsletterDigestResult>> digestBuilder)
    {
        var subscribers = await GetSubscribersAsync();

        foreach (var sub in subscribers)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sub.Email))
                {
                    _logger.LogWarning("Skipping {UserId}: missing email", sub.RowKey);
                    continue;
                }

                var digest = await digestBuilder(sub);
                await _emailSender.SendNewsletterAsync(sub.Email, digest);
                _logger.LogInformation("Sent digest to {Email} Tier={Tier} Category={Category}", sub.Email, digest.Tier, digest.Category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending to {UserId}", sub.RowKey);
            }
        }
    }
}
