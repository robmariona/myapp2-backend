using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<IdentityUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByNameAsync(model.Username);
        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
        {
            var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

            // THE FIX: Use SigningCredentials instead of just a key
            var token = new JwtSecurityToken(
                issuer: null, // You can add your site URL here later
                audience: null,
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
        return Unauthorized();
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        var userExists = await _userManager.FindByNameAsync(model.Username);
        if (userExists != null)
            return BadRequest("User already exists!");

        IdentityUser user = new()
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Username
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            return BadRequest("User creation failed! Ensure password has uppercase, number, and symbol.");

        return Ok("User created successfully!");
    }

    public class RegisterModel
    {
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

}

public class LoginModel { public string Username { get; set; } = ""; public string Password { get; set; } = ""; }