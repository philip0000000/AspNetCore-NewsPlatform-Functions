using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveNewsFunction.Models
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

