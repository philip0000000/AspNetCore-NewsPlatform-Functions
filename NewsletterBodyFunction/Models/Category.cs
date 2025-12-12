using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFHP_NewsSite.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty; // admin note

        public bool IsActive { get; set; } = true;

        // A category can have many articles
        public virtual ICollection<Article> Articles { get; set; } = new List<Article>();

    }
}
