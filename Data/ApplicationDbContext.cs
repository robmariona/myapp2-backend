using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using myapp2.Models;

namespace myapp2.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Products> Products { get; set; }
        public DbSet<Insurance> Insurances { get; set; }
        public DbSet<Claim> Claims { get; set; } // Add this!

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Force the app to ignore the "Pending Changes" check and just start up
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }




        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Set precision for Products.Price (18 digits total, 2 after the decimal)
            builder.Entity<Products>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            // Set precision for Insurance properties
            builder.Entity<Insurance>()
                .Property(i => i.CoverageAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Insurance>()
                .Property(i => i.PremiumPrice)
                .HasColumnType("decimal(18,2)");

        }
    }
}
