using Microsoft.AspNetCore.Mvc;

namespace myapp2.Models.ViewModels
{
    public class InsuranceReportViewModel
    {
        public int TotalPolicies { get; set; }
        public decimal TotalPremiumRevenue { get; set; }
        public List<InsuranceSummaryDto> TopInsurances { get; set; }
    }

    public record InsuranceSummaryDto(string ProductName, string PolicyName, decimal Premium);
}
