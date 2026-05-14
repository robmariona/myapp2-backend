using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using myapp2.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using myapp2.Services;
using DotNetEnv;                    // ← Add this
using System;                       // ← For StringComparison

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load(); // Load .env file

// --- 1. DATABASE CONFIGURATION ---
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? Environment.GetEnvironmentVariable("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

// Auto-detect which database provider to use
if (connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) ||
    connectionString.Contains("Trusted_Connection=", StringComparison.OrdinalIgnoreCase))
{
    // Local Development → SQL Server
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    // Production → Supabase PostgreSQL
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// --- 2. JWT AUTHENTICATION ---
var jwtSecret = builder.Configuration["JWT:Secret"]
             ?? Environment.GetEnvironmentVariable("JWT_SECRET")
             ?? "ThisIsASecretKeyThatIsAtLeast32CharactersLong!!";

var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

// --- 3. CORS ---
var allowedOrigins = new[]
{
    "http://localhost:5173",
    "http://localhost:3000",
    "https://myapp2-ui.onrender.com",        // ← Future frontend
    "https://myapp2-backend-72uk.onrender.com" // your current backend
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddScoped<IInsuranceService, InsuranceService>();

var app = builder.Build();

// --- 4. MIDDLEWARE ---
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();

// --- 5. ENDPOINTS ---
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapControllers();

app.Run();