using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AFHP_NewsSite.Models
{
    // Unique composite index on (ClaimType, ClaimValue, Category)
    [Index(nameof(ClaimType), nameof(ClaimValue), nameof(Category),
           IsUnique = true, Name = "UX_ClaimMasters_TypeValueCategory")]
    public class ClaimMaster
    {
        [Key]
        public Guid Id { get; set; }

        [Required, StringLength(200)]
        public string ClaimType { get; set; } = null!;  // e.g., "Permission"

        [Required, StringLength(200)]
        public string ClaimValue { get; set; } = null!;

        // Where the claim is allowed to be assigned
        // Allowed: "Role", "User", "Both", "Subscription"
        [Required, StringLength(64)]
        public string Category { get; set; } = "Both"; //User, Role, Both,Subscription

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedOn { get; set; }
    }
}
