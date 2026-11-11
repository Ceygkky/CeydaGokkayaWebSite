using CeydaPortfolio.Models;
using Microsoft.EntityFrameworkCore;

namespace CeydaPortfolio.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<PageView> PageViews { get; set; }
    }
}