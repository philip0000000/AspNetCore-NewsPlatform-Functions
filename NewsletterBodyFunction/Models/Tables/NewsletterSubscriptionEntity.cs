//using AFHP_NewsSite.Enums;
//using AFHP_NewsSite.Models;
//using Azure;
//using Azure.Data.Tables;


//public class NewsletterSubscriptionEntity : ITableEntity
//{
//    public string PartitionKey { get; set; } = "Newsletter";
//    public string RowKey { get; set; } = string.Empty; // UserId
//    public DateTimeOffset? Timestamp { get; set; }
//    public ETag ETag { get; set; }

//    public string Email { get; set; } = string.Empty;//Added email property
//    public bool IsSubscribed { get; set; } = true;

//    public string Tier { get; set; } = SubscriptionTier.Basic.ToString();

//    public string? Category { get; set; } = NewsletterCategory.Local.ToString();
//    public DateTime? SubscribedOn { get; set; } = DateTime.UtcNow;
//    public DateTime? UnsubscribedOn { get; set; }
//}
using AFHP_NewsSite.Enums;
using AFHP_NewsSite.Models;
using Azure;
using Azure.Data.Tables;

public class NewsletterSubscriptionEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "Newsletter";
    public string RowKey { get; set; } = string.Empty; // UserId
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Email { get; set; } = string.Empty; // Added email property
    public bool IsSubscribed { get; set; } = true;

    // Backing storage for Tier (string in Table Storage)
    public string TierString { get; set; } = SubscriptionTier.Basic.ToString();

    // Strongly typed property for Tier
    public SubscriptionTier Tier
    {
        get => Enum.TryParse<SubscriptionTier>(TierString, true, out var result) ? result : SubscriptionTier.Basic;
        set => TierString = value.ToString();
    }

    // Backing storage for Category (string in Table Storage)
    public string? CategoryString { get; set; } = NewsletterCategory.Local.ToString();

    // Strongly typed property for Category
    public NewsletterCategory Category
    {
        get => Enum.TryParse<NewsletterCategory>(CategoryString, true, out var result) ? result : NewsletterCategory.Local;
        set => CategoryString = value.ToString();
    }
    
    public DateTime? SubscribedOn { get; set; } = DateTime.UtcNow;
    public DateTime? UnsubscribedOn { get; set; }
}



