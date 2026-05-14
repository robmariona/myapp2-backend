using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace myapp2.Models
{
    public class Products : BaseEntity
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Category { get; set; }
        public DateTime DueDate { get; set; }
        public List<Insurance> Insurances { get; set; } = new();
    }

    public record ProductRequest(string Nombre, decimal Price, string Description, string Category);
    // Updating the record/class to include the new fields
    public record ProductResponse(
        int Id,
        string Nombre,
        decimal Price,
        bool HasInsurances,
        string Category = "General",    // Added this
        string Description = ""         // Added this
    );
}
