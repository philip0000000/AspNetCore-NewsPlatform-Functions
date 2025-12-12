using AFHPNewsFunctions.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFHPNewsFunctions.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public PaymentProcessor PaymentProcessor { get; set; } // Stored as string: FreeTrial, Card, Stripe, etc

        [StringLength(255)]
        public string CustomerId { get; set; } = string.Empty;

        [StringLength(255)]
        public string SubscriptionId { get; set; } = string.Empty;

        [StringLength(255)]
        public string PriceId { get; set; } = string.Empty;

        [Required]
        public DateTime PaidUntilUtc { get; set; }

        public DateTime? NextBillingUtc { get; set; }

        [NotMapped]
        public bool Expired => DateTime.UtcNow > PaidUntilUtc;
        [NotMapped]
        public bool Cancelled => NextBillingUtc == null;
    }
}
