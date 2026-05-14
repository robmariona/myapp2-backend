using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using myapp2.Data;
using myapp2.Models;

namespace myapp2.API
{
    [Route("api/Products")] // Only ONE route here
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll()
        {
            // 1. Fetch raw data from DB (EF Core loves anonymous objects)
            var productData = await _context.Products
                .Select(p => new {
                    p.Id,
                    p.Nombre,
                    p.Price,
                    HasInsurances = p.Insurances.Any(),
                    Category = p.Category ?? "General",
                    Description = p.Description ?? ""
                })
                .ToListAsync();

            // 2. Map to your DTO in memory (C# logic)
            var response = productData.Select(p => new ProductResponse(
                p.Id,
                p.Nombre,
                p.Price,
                p.HasInsurances,
                p.Category,
                p.Description
            ));

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _context.Products
                .Include(p => p.Insurances)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Nombre))
                    return BadRequest("Product Name is required.");

                var product = new Products
                {
                    Nombre = request.Nombre,
                    Price = request.Price,
                    Description = request.Description ?? "No description provided",
                    Category = request.Category ?? "General",
                    DueDate = DateTime.Now.AddYears(1),
                    Insurances = new List<Insurance>()
                };

                if (product.Price > 1000)
                {
                    product.Insurances.Add(new Insurance
                    {
                        PolicyName = "Automatic High-Value Cover",
                        InsuranceType = "Auto-Generated",
                        PremiumPrice = product.Price * 0.05m,
                        CoverageAmount = product.Price
                    });
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    id = product.Id,
                    nombre = product.Nombre,
                    price = product.Price,
                    description = product.Description,
                    category = product.Category
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB ERROR: {ex.Message}");
                return StatusCode(500, $"Database Error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ProductRequest request)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            // MAP EVERY FIELD HERE
            product.Nombre = request.Nombre;
            product.Price = request.Price;
            product.Description = request.Description;
            product.Category = request.Category; // <-- THIS IS LIKELY WHAT WAS MISSING

            // This triggers the UPDATE SQL command
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Product deleted" });
        }
    }
}