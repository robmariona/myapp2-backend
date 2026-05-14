using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace myapp2.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        // Use your real SQL connection string here
        optionsBuilder.UseSqlServer("Server=DESKTOP-D74S2MB\\SQLEXPRESS;Database=Almacen;Trusted_Connection=True;TrustServerCertificate=True;");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}