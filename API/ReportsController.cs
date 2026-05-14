using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using myapp2.Data;
using static myapp2.Models.Claim;
using Microsoft.AspNetCore.Authorization;

namespace myapp2.API
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context) => _context = context;

        [HttpGet("claims-by-category")]
        public async Task<IActionResult> GetClaimsByCategory()
        {
            var report = await _context.Claims
                .Include(c => c.Insurance)
                    .ThenInclude(i => i.Product)
                .GroupBy(c => c.Insurance.Product.Category ?? "Uncategorized")
                .Select(g => new ClaimSummary(
                    g.Key,
                    g.Count(),
                    g.Sum(c => c.Insurance.CoverageAmount)
                ))
                .ToListAsync();

            return Ok(report);
        }

        [HttpGet("financial-health")]
        public async Task<IActionResult> GetFinancialHealth()
        {
            // Example fix for the ReportsController
            var totalPremiums = await _context.Insurances.SumAsync(i => (decimal?)i.PremiumPrice) ?? 0m;
            var totalCoverage = await _context.Insurances.SumAsync(i => i.CoverageAmount);

            // Profit margin calculation (Steroid logic)
            decimal margin = totalPremiums > 0 ? ((totalPremiums / totalCoverage) * 100) : 0;

            return Ok(new FinancialHealthReport(totalPremiums, totalCoverage, Math.Round(margin, 2)));
        }
    }
}
