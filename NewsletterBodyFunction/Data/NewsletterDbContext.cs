using AFHP_NewsSite.Models;
using Microsoft.EntityFrameworkCore;

namespace AFHPNewsFunctions.Data
{
    // Minimal DbContext for newsletter function
    public class NewsletterDbContext : DbContext
    {
        public NewsletterDbContext(DbContextOptions<NewsletterDbContext> options)
            : base(options)
        {
        }

        // Subscriptions table
        //public DbSet<Subscription> Subscriptions { get; set; } = default!;

        // Articles table (if you want to pull content directly)
        public DbSet<Article> Articles { get; set; } = default!;

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    // Example: ensure PublishedDate defaults to UTC now
        //    modelBuilder.Entity<Article>()
        //        .Property(a => a.PublishedDate)
        //        .HasDefaultValueSql("SYSUTCDATETIME()")
        //        .ValueGeneratedOnAdd();

        //    // Example: enforce unique email per subscription
        //    modelBuilder.Entity<Subscription>()
        //        .HasIndex(s => s.Email)
        //        .IsUnique();
        //}
    }
}
