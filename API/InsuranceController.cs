using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using myapp2.Data;
using myapp2.Models;
using myapp2.Services;
using myapp2.Models.ViewModels;

namespace myapp2.API;

[Route("api/[controller]")]
[ApiController]
public class InsurancesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IInsuranceService _insuranceService;

    public InsurancesController(ApplicationDbContext context, IInsuranceService insuranceService){

        _context = context;
        _insuranceService = insuranceService; // 2. CRITICAL: Must assign it here!
    } 

    // FIX: Added this method so the Dashboard GET request works!
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var insurances = await _context.Insurances.ToListAsync();
        return Ok(insurances);
    }

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetByProduct(int productId)
    {
        var product = await _context.Products
            .Include(p => p.Insurances)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null) return NotFound("Product not found.");

        return Ok(product.Insurances);
    }

    [HttpPost]
    public async Task<IActionResult> CreateInsurance([FromBody] InsuranceRequest request)
    {

        // 1. Controller handles "External" validation
        if (request.CoverageAmount < 0)
            return BadRequest("Coverage cannot be negative.");

        var product = await _context.Products.FindAsync(request.ProductId);
        if (product == null) return NotFound("Product not found.");

        // 2. Service handles "Internal" business logic
        decimal calculatedPremium = _insuranceService.CalculatePremium(product.Price);

        
       

        // Insurance costs 2% of the item price per year
        // USE THE SERVICE INSTEAD OF DOING MATH HERE
        
        decimal coverage = _insuranceService.CalculateCoverage(product.Price, request.CoverageAmount);

        var insurance = new Insurance
        {
            ProductId = request.ProductId,
            PolicyName = $"{request.InsuranceType} Policy for {product.Nombre}",
            InsuranceType = request.InsuranceType,
            PremiumPrice = calculatedPremium,
            CoverageAmount = request.CoverageAmount > 0 ? request.CoverageAmount : product.Price
        };

        _context.Insurances.Add(insurance);
        await _context.SaveChangesAsync();

        return Ok(new {
            id = insurance.Id,
            policyName = insurance.PolicyName,
            insuranceType = insurance.InsuranceType,
            premiumPrice = insurance.PremiumPrice,
            coverageAmount = insurance.CoverageAmount,
            productId = insurance.ProductId
        });
    }
    [HttpGet("report")]
    public async Task<IActionResult> SummaryReport()
    {
        var insurances = await _context.Insurances.Include(i => i.Product).ToListAsync();

        var model = new InsuranceReportViewModel
        {
            TotalPolicies = insurances.Count,
            TotalPremiumRevenue = insurances.Sum(i => i.PremiumPrice),
            TopInsurances = insurances.Select(i => new InsuranceSummaryDto(
                i.Product.Nombre, i.PolicyName, i.PremiumPrice)).ToList()
        };

        return Ok(model); // Or return Ok(model) if it's an API
    }


}

// DTO for cleaner API communication
public record InsuranceRequest(int ProductId, string InsuranceType, decimal CoverageAmount);