using AFHP_NewsSite.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFHP_NewsSite.Models
{
    public class Subscription
    {
        [Key] public int Id { get; set; }

        [Required, Precision(18, 2)]
        public decimal Price { get; set; }

        [Required] public DateTime Created { get; set; } = DateTime.UtcNow;
        [Required] public DateTime Expires { get; set; }

        // FKs
        [Required] public int SubscriptionTypeId { get; set; }
        [Required] public string UserId { get; set; } // FK to Identity user (string)
        public Guid? ClaimMasterId { get; set; }
        public int PaymentId { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual SubscriptionType SubscriptionType { get; set; }
        [ForeignKey(nameof(ClaimMasterId))]
        public virtual ClaimMaster? ClaimMaster { get; set; }
        public virtual Payment Payment { get; set; }

        [Required] public bool IsPaid { get; set; }

        [Required] public bool IsActive { get; set; }
    }
}
