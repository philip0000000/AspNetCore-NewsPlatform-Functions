using ArchiveNewsFunction.Functions;
using ArchiveNewsFunction.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveNewsFunction.Models
{
    public class Articles
    {
        [Key]
        public int Id { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? PublishedTime { get; set; } // Set when publish happens

        [Required]
        [StringLength(255)]
        public string Slug { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Status")]
        public int ArticleStatusId { get; set; }

        [ForeignKey(nameof(ArticleStatusId))]
        public virtual ArticleStatus ArticleStatus { get; set; } = null!;
    }
}
