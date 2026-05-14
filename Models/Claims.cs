using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace myapp2.Models
{

    public class Claim : BaseEntity
    {
        [Required]
        [MinLength(10, ErrorMessage = "Description is too short")] // This is likely the culprit
        public string Description { get; set; }
        public DateTime DateFiled { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending";

        // Relationship to Insurance
        public int InsuranceId { get; set; }
        public Insurance? Insurance { get; set; }
        public record ClaimSummary(string Category, int TotalClaims, decimal TotalClaimValue);
        public record FinancialHealthReport(decimal TotalPremiumsCollected, decimal TotalCoverageLiability, decimal ProfitMargin);



    }

    

}
