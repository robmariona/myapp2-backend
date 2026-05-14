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
var connectionString = Environment.GetEnvironmentVariable("DefaultConnection");



DotNetEnv.Env.Load(); // Load .env file

// --- 1. DATABASE CONFIGURATION ---
if (!string.IsNullOrEmpty(connectionString))
{
    // Production (Supabase)
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    // Local dev (SQL Server)
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
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

// --- 3. CORS (Updated with your live URL) ---
var allowedOrigins = new[]
{
    "http://localhost:5173",
    "http://localhost:3000",
    "https://myapp2-ui.onrender.com",           // ← Your live frontend
    "https://myapp2-ui-*.onrender.com",         // ← Safety for Render
    "https://myapp2-backend-72uk.onrender.com"
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
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

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