//using Microsoft.EntityFrameworkCore;
//using System.Collections.Generic;
//using System.Reflection.Emit;


//namespace NewsletterFunctionApp.Data
//{
//    public class NewsletterDbContext : DbContext
//    {
//        public NewsletterDbContext(DbContextOptions<NewsletterDbContext> options)
//            : base(options)
//        {
//        }

//        // Only entities needed for newsletter content and subscription logic
//        public DbSet<Article> Articles => Set<Article>();
//        public DbSet<Category> Categories => Set<Category>();
//        public DbSet<Subscription> Subscriptions => Set<Subscription>();
//        public DbSet<SubscriptionType> SubscriptionTypes => Set<SubscriptionType>();
//        public DbSet<ArticleStatus> ArticleStatuses => Set<ArticleStatus>();

//        protected override void OnModelCreating(ModelBuilder b)
//        {
//            base.OnModelCreating(b);

//            // Unique constraints
//            b.Entity<Article>().HasIndex(x => x.Slug).IsUnique();
//            b.Entity<Category>().HasIndex(x => x.Name).IsUnique();

//            // Article relationships
//            b.Entity<Article>()
//             .HasOne(a => a.Author)
//             .WithMany()
//             .HasForeignKey(a => a.AuthorId)
//             .OnDelete(DeleteBehavior.Restrict);

//            // Seed ArticleStatus values
//            b.Entity<ArticleStatus>().HasData(
//                new ArticleStatus { Id = 1, StatusName = "Draft" },
//                new ArticleStatus { Id = 2, StatusName = "Review" },
//                new ArticleStatus { Id = 3, StatusName = "Approved" },
//                new ArticleStatus { Id = 4, StatusName = "Published" },
//                new ArticleStatus { Id = 5, StatusName = "Archived" },
//                new ArticleStatus { Id = 6, StatusName = "Rejected" }
//            );
//        }
//    }
//}
