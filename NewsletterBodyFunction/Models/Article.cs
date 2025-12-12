using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFHP_NewsSite.Models
{
    public class Article
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string AuthorId { get; set; }

        [ForeignKey(nameof(AuthorId))]
        public ApplicationUser Author { get; set; }


        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedTime { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime LastEditedTime { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? PublishedTime { get; set; } // Set when publish happens

        [Required]
        [StringLength(255)]
        public string Slug { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Headline { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string ContentSummary { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.MultilineText)] // lets EF Core create the column as nvarchar(max) in SQL Server, meaning no hard limit
        public string Content { get; set; } = string.Empty;

        [DataType(DataType.MultilineText)]
        public string? FeedbackNote {  get; set; }

        public int Views { get; set; } = 0;

        // Stored like counter (cached). Real likes come from ArticleLikes table.
        // This value is updated after each toggle so sorting stays fast.
        public int Likes { get; set; } = 0;

        [Required]
        [Display(Name = "Image URL")]
        [Url]
        public string ImageUrl { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public virtual Category Category { get; set; }

        [Required]
        [Display(Name = "Status")]
        public int ArticleStatusId { get; set; }

        [ForeignKey(nameof(ArticleStatusId))]
        public virtual ArticleStatus ArticleStatus { get; set; } = null!;

        [Required]
        [Display(Name = "Editor’s Choice")]
        public bool IsEditorChoice { get; set; } = false;
    }
}
