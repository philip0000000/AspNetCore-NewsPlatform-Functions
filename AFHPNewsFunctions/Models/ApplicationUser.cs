using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFHPNewsFunctions.Models
{
    // User (extends IdentityUser) — adds FirstName, LastName, DOB, and List<Subscription>
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Display(Name = "First name")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last name")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateOnly? DOB { get; set; } // (Date of Birth)

        [DataType(DataType.DateTime)]
        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Not stored in DB. We compute it when needed.
        // True if the user has at least one valid sub right now.
        // We check: paid, marked active, and not expired.
        [NotMapped]
        public bool HasSubscription =>
            Subscriptions?.Any(s =>
                s.IsPaid &&                            // payment done
                s.IsActive &&                           // still active
                s.Expires > DateTime.UtcNow) == true;   // end date in the future

        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
