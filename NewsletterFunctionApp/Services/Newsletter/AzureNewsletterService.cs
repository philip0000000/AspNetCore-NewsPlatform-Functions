//using Azure;
//using Azure.Data.Tables;
//using Microsoft.Extensions.Configuration;
//using NewsletterFunctionApp.Services.Newsletter;
//using AFHPNewsFunctions.Services;

//public class AzureNewsletterService : INewsletterService
//{
//    private readonly TableClient _tableClient;
//    private readonly IEmailSender _emailSender;

//    public AzureNewsletterService(
//        IConfiguration config,
//        IEmailSender emailSender)
//    {
//        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));

//        var connectionString = config["AzureWebJobsStorage"]
//            ?? throw new ArgumentNullException("AzureWebJobsStorage connection string is not configured.");

//        var tableServiceClient = new TableServiceClient(connectionString);
//        _tableClient = tableServiceClient.GetTableClient("NewsletterSubscriptions");
//        _tableClient.CreateIfNotExists();
//    }

//    public async Task SubscribeAsync(string email, string tier = "Basic", string? category = null)
//    {
//        var entity = new NewsletterSubscriptionEntity
//        {
//            PartitionKey = "Newsletter",
//            RowKey = email, // ✅ store email directly
//            IsSubscribed = true
//        };

//        await _tableClient.UpsertEntityAsync(entity);
//    }

//    public async Task UnsubscribeAsync(string email)
//    {
//        try
//        {
//            var response = await _tableClient.GetEntityAsync<NewsletterSubscriptionEntity>("Newsletter", email);
//            var entity = response.Value;

//            entity.IsSubscribed = false;
//            await _tableClient.UpdateEntityAsync(entity, entity.ETag);
//        }
//        catch (RequestFailedException ex)
//        {
//            Console.WriteLine($"Unsubscribe failed for {email}: {ex.Message}");
//        }
//    }

//    public async Task<bool> IsSubscribedAsync(string email)
//    {
//        try
//        {
//            var response = await _tableClient.GetEntityAsync<NewsletterSubscriptionEntity>("Newsletter", email);
//            return response.Value.IsSubscribed;
//        }
//        catch
//        {
//            return false;
//        }
//    }

//    public async Task<IEnumerable<NewsletterSubscriptionEntity>> GetSubscribersAsync(string? tier = null, string? category = null)
//    {
//        var query = _tableClient.QueryAsync<NewsletterSubscriptionEntity>(e => e.IsSubscribed);
//        var results = new List<NewsletterSubscriptionEntity>();

//        await foreach (var entity in query)
//        {
//            results.Add(entity);
//        }

//        return results;
//    }

//    public async Task SendConfirmationEmailAsync(string email, bool subscribed, string? tier = null)
//    {
//        if (string.IsNullOrWhiteSpace(email)) return;

//        var subject = subscribed ? "Newsletter Subscription Confirmed" : "Newsletter Unsubscribed";
//        var body = subscribed
//            ? $"Welcome aboard! You are subscribed{(string.IsNullOrEmpty(tier) ? "" : $" as {tier}")}."
//            : "You’ve successfully unsubscribed. We’ll miss you!";

//        await _emailSender.SendEmailAsync(email, subject, body);
//    }

//    public async Task SendWeeklyNewsletterAsync(Func<NewsletterSubscriptionEntity, Task<string>> bodyBuilder)
//    {
//        var subscribers = await GetSubscribersAsync();

//        foreach (var sub in subscribers)
//        {
//            try
//            {
//                var body = await bodyBuilder(sub);

//                await _emailSender.SendEmailAsync(
//                    sub.RowKey, // ✅ RowKey holds the email
//                    "Your Weekly Newsletter",
//                    body
//                );
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error sending to {sub.RowKey}: {ex.Message}");
//            }
//        }
//    }
//}
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using NewsletterFunctionApp.Services.Newsletter;
using AFHPNewsFunctions.Services;

public class AzureNewsletterService : INewsletterService
{
    private readonly TableClient _tableClient;
    private readonly IEmailSender _emailSender;

    public AzureNewsletterService(
        IConfiguration config,
        IEmailSender emailSender)
    {
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));

        var connectionString = config["AzureWebJobsStorage"]
            ?? throw new ArgumentNullException("AzureWebJobsStorage connection string is not configured.");

        var tableServiceClient = new TableServiceClient(connectionString);
        _tableClient = tableServiceClient.GetTableClient("NewsletterSubscriptions");
        _tableClient.CreateIfNotExists();
    }

    public async Task SubscribeAsync(string userId, string email, string tier = "Basic", string? category = null)
    {
        var entity = new NewsletterSubscriptionEntity
        {
            PartitionKey = "Newsletter",
            RowKey = userId,   // ✅ stable UserId
            Email = email,     // ✅ dedicated property
            IsSubscribed = true,
            Tier = tier,
            Category = category,
            SubscribedOn = DateTime.UtcNow
        };

        await _tableClient.UpsertEntityAsync(entity);
    }

    public async Task UnsubscribeAsync(string userId)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<NewsletterSubscriptionEntity>("Newsletter", userId);
            var entity = response.Value;

            entity.IsSubscribed = false;
            entity.UnsubscribedOn = DateTime.UtcNow;

            await _tableClient.UpdateEntityAsync(entity, entity.ETag);
        }
        catch (RequestFailedException ex)
        {
            Console.WriteLine($"Unsubscribe failed for {userId}: {ex.Message}");
        }
    }

    public async Task<bool> IsSubscribedAsync(string userId)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<NewsletterSubscriptionEntity>("Newsletter", userId);
            return response.Value.IsSubscribed;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IEnumerable<NewsletterSubscriptionEntity>> GetSubscribersAsync(string? tier = null, string? category = null)
    {
        var query = _tableClient.QueryAsync<NewsletterSubscriptionEntity>(e => e.IsSubscribed);
        var results = new List<NewsletterSubscriptionEntity>();

        await foreach (var entity in query)
        {
            if ((tier == null || entity.Tier == tier) &&
                (category == null || entity.Category == category))
            {
                results.Add(entity);
            }
        }

        return results;
    }

    public async Task SendConfirmationEmailAsync(string email, bool subscribed, string? tier = null)
    {
        if (string.IsNullOrWhiteSpace(email)) return;

        var subject = subscribed ? "Newsletter Subscription Confirmed" : "Newsletter Unsubscribed";
        var body = subscribed
            ? $"Welcome aboard! You are subscribed{(string.IsNullOrEmpty(tier) ? "" : $" as {tier}")}."
            : "You’ve successfully unsubscribed. We’ll miss you!";

        await _emailSender.SendEmailAsync(email, subject, body);
    }

    public async Task SendWeeklyNewsletterAsync(Func<NewsletterSubscriptionEntity, Task<string>> bodyBuilder)
    {
        var subscribers = await GetSubscribersAsync();

        foreach (var sub in subscribers)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sub.Email))
                {
                    Console.WriteLine($"Skipping {sub.RowKey}: missing email.");
                    continue;
                }

                var body = await bodyBuilder(sub);

                await _emailSender.SendEmailAsync(
                    sub.Email, // ✅ use Email property
                    "Your Weekly Newsletter",
                    body
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending to {sub.RowKey}: {ex.Message}");
            }
        }
    }
}

