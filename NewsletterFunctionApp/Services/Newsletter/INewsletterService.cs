namespace NewsletterFunctionApp.Services.Newsletter
{
    public interface INewsletterService
    {
        // Subscription management
        Task SubscribeAsync(string userId, string email, string tier = "Basic", string? category = null);//Note: added email parameter
        Task UnsubscribeAsync(string userId);
        Task<bool> IsSubscribedAsync(string userId);

        // Subscriber queries
        Task<IEnumerable<NewsletterSubscriptionEntity>> GetSubscribersAsync(string? tier = null, string? category = null);

        // Confirmation emails
        Task SendConfirmationEmailAsync(string email, bool subscribed, string? tier = null);

        // Weekly newsletter delivery
        Task SendWeeklyNewsletterAsync(Func<NewsletterSubscriptionEntity, Task<string>> bodyBuilder);
    }
}

