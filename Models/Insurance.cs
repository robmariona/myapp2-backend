using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace myapp2.Models
{
    public class Insurance : BaseEntity
    {
        public string PolicyName { get; set; } = string.Empty;
        public decimal CoverageAmount { get; set; }
        public decimal PremiumPrice { get; set; }
        public string InsuranceType { get; set; } = string.Empty;

        // Relationship to Product
        public int ProductId { get; set; }
        public Products? Product { get; set; }

        // Relationship to Claims
        public List<Claim> Claims { get; set; } = new();
    }
}
