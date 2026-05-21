using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using myapp2.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using myapp2.Services;
using DotNetEnv;
using System;

var builder = WebApplication.CreateBuilder(args);

// Load .env file
DotNetEnv.Env.Load();

// --- 1. DATABASE CONFIGURATION ---
// Use the PostgresConnection key consistently
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
                       ?? builder.Configuration.GetConnectionString("PostgresConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString); // Make sure this is UseNpgsql, not UseSqlServer
});

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
    "https://myapp2-ui.onrender.com",
    "https://myapp2-ui-*.onrender.com",
    "https://myapp2-backend-72uk.onrender.com"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .SetIsOriginAllowedToAllowWildcardSubdomains()
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddScoped<IInsuranceService, InsuranceService>();
builder.Services.AddControllers();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var app = builder.Build();

// --- 4. AUTOMATIC MIGRATIONS ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // This applies the migration to your database (Postgres) on startup
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

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