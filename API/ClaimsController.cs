
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using myapp2.Data;
using myapp2.Models;
using Microsoft.AspNetCore.Authorization;

namespace myapp2.API;


[Authorize] // Only logged-in admins can see all claims
[Route("api/[controller]")]
[ApiController]
public class ClaimsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ClaimsController(ApplicationDbContext context) => _context = context;
    [HttpGet("admin/all")]
    public async Task<ActionResult<IEnumerable<object>>> GetAllClaimsAdmin()
    {
        // Using an anonymous object to "Shape" the data for the UI
        return await _context.Claims
            .Include(c => c.Insurance)
            .Select(c => new {
                c.Id,
                c.Description,
                c.DateFiled,
                c.Status,
                PolicyName = c.Insurance.PolicyName,
                InsuranceType = c.Insurance.InsuranceType
            })
            .ToListAsync();
    }

    [Authorize]
    [HttpPost]
    // Create a small class inside the controller or use the Claim model
    public async Task<IActionResult> CreateClaim(int id, [FromBody] ClaimRequest request)
    {
        var insurance = await _context.Insurances
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.Id == request.InsuranceId);

        if (insurance == null) return NotFound("Insurance policy not found.");

        var claim = new Claim
        {
            InsuranceId = request.InsuranceId,
            Description = request.Description,
            DateFiled = DateTime.Now,
            Status = "Pending"
        };

        _context.Claims.Add(claim);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Claim filed successfully", ClaimId = claim.Id });
    }

    // Add this helper class at the bottom of the file or in your Models folder
    public class ClaimRequest
    {
        public int InsuranceId { get; set; }
        public string Description { get; set; }
    }

    // Get all claims for a specific Product (Searching through the relationship)
    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetClaimsByProduct(int productId)
    {
        var claims = await _context.Claims
            .Include(c => c.Insurance)
            .Where(c => c.Insurance.ProductId == productId)
            .ToListAsync();

        return Ok(claims);
    }

    public async Task CreateClaim(int id, string v)
    {
        throw new NotImplementedException();
    }
}