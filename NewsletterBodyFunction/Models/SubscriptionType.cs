using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AFHP_NewsSite.Models
{
    public class SubscriptionType
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string TypeName { get; set; } = string.Empty;

        [Required, StringLength(250)]
        public string Description { get; set; } = string.Empty;

        [Required, Precision(18, 2)]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
