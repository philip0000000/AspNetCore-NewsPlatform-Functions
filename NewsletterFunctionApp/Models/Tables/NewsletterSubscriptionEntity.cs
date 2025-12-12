using Azure;
using Azure.Data.Tables;

public class NewsletterSubscriptionEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "Newsletter";
    public string RowKey { get; set; } = string.Empty; // UserId
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Email { get; set; } = string.Empty;
    public bool IsSubscribed { get; set; } = true;
    public string Tier { get; set; } = "Basic";
    public string? Category { get; set; }
    public DateTime? SubscribedOn { get; set; } = DateTime.UtcNow;
    public DateTime? UnsubscribedOn { get; set; }
}


