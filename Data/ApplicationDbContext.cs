using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using myapp2.Models;

namespace myapp2.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Products> Products { get; set; }
        public DbSet<Insurance> Insurances { get; set; }
        public DbSet<Claim> Claims { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            //optionsBuilder.UseNpgsql("Host=localhost;Database=dummy;Username=postgres;Password=password");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Fix Decimal Precision for PostgreSQL
            // Npgsql prefers "numeric" or just letting it handle the decimal mapping
            builder.Entity<Products>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            builder.Entity<Insurance>()
                .Property(i => i.CoverageAmount)
                .HasPrecision(18, 2);

            builder.Entity<Insurance>()
                .Property(i => i.PremiumPrice)
                .HasPrecision(18, 2);

            // 2. THE IDENTITY FIX: Convert all string mappings from nvarchar to text
            // This prevents the "type nvarchar does not exist" error during migration generation
            foreach (var entity in builder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    if (property.GetColumnType()?.Contains("nvarchar") == true)
                    {
                        property.SetColumnType("text");
                    }
                }
            }
        }
    }
}