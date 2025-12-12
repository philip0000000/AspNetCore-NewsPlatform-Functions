using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AFHPNewsFunctions.Models;

namespace AFHPNewsFunctions.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Subscription> Subscriptions {  get; set; }
        public DbSet<SubscriptionType> SubscriptionTypes {  get; set; }

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);
          
            // map to existing table name
            b.Entity<ClaimMaster>().ToTable("ClaimMaster");

            // Link Subscription to ClaimMaster safely; prevent accidental cascade deletes
            b.Entity<Subscription>()
             .HasOne(s => s.ClaimMaster)
             .WithMany()
             .HasForeignKey(s => s.ClaimMasterId)
             .OnDelete(DeleteBehavior.Restrict);

            // Ensure RegistrationDate defaults to current UTC in the DB
            b.Entity<ApplicationUser>()
             .Property(u => u.RegistrationDate)
             .HasDefaultValueSql("SYSUTCDATETIME()") // or "GETUTCDATE()"
             .ValueGeneratedOnAdd();

            b.Entity<Payment>()
            .Property(p => p.PaymentProcessor)
            .HasConversion<string>();
        }
    }
}
