using ArchiveNewsFunction.Models;
using Microsoft.EntityFrameworkCore;
using ArchiveNewsFunction.Functions; // Adjust namespace to where your entities live

namespace ArchiveNewsFunction.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Articles> Articles { get; set; }
        public DbSet<ArticleStatus> ArticleStatuses { get; set; }
    }
}
