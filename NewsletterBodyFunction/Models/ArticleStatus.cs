using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFHP_NewsSite.Models
{
    [Table("ArticleStatus")]
    public class ArticleStatus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string StatusName { get; set; } = string.Empty;
    }
}
