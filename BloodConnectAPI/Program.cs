using BloodConnectAPI.DataAccess.Data;
using BloodConnectAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme             = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]
        ?? throw new InvalidOperationException("JwtSettings:Key is missing"));

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSettings["Issuer"],
        ValidAudience            = jwtSettings["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(key),
        ClockSkew                = TimeSpan.Zero
    };
});


// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                // Vite dev server (default)
                "http://localhost:5173",
                "https://localhost:5173",
                // Webpack / CRA / other bundlers
                "http://localhost:8080",
                "https://localhost:8080",
                // fallback port if 8080 is busy
                "http://localhost:8081",
                "https://localhost:8081",
                // Create React App default
                "http://localhost:3000",
                "https://localhost:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    // إصلاح IFormFile
    options.AddSchemaTransformer((schema, context, cancellationToken) =>
    {
        if (context.JsonTypeInfo.Type == typeof(IFormFile))
        {
            schema.Type = Microsoft.OpenApi.JsonSchemaType.String;
            schema.Format = "binary";
            schema.Properties?.Clear();
        }
        return Task.CompletedTask;
    });
});

// AutoMapper
builder.Services.AddAutoMapper(cfg =>
    cfg.AddMaps(typeof(BloodConnectAPI.Service.Implementations.DonorService).Assembly));

// Repository / Unit of Work
builder.Services.AddScoped<BloodConnectAPI.DataAccess.Repositories.Interfaces.IUnitOfWork, BloodConnectAPI.DataAccess.Repositories.Implementations.UnitOfWork>();

// Services
builder.Services.AddScoped<BloodConnectAPI.Service.Interfaces.IAuthService, BloodConnectAPI.Service.Implementations.AuthService>();
builder.Services.AddScoped<BloodConnectAPI.Service.Interfaces.IUserService, BloodConnectAPI.Service.Implementations.UserService>();
builder.Services.AddScoped<BloodConnectAPI.Service.Interfaces.IDonorService, BloodConnectAPI.Service.Implementations.DonorService>();
builder.Services.AddScoped<BloodConnectAPI.Service.Interfaces.IDonationService, BloodConnectAPI.Service.Implementations.DonationService>();
builder.Services.AddScoped<BloodConnectAPI.Service.Interfaces.IInventoryService, BloodConnectAPI.Service.Implementations.InventoryService>();
builder.Services.AddScoped<BloodConnectAPI.Service.Interfaces.IBloodRequestService, BloodConnectAPI.Service.Implementations.BloodRequestService>();
builder.Services.AddScoped<BloodConnectAPI.Service.Interfaces.IDonorResponseService, BloodConnectAPI.Service.Implementations.DonorResponseService>();
builder.Services.AddScoped<BloodConnectAPI.Service.Interfaces.IDonorMedicalDocumentService, BloodConnectAPI.Service.Implementations.DonorMedicalDocumentService>();
builder.Services.AddScoped<BloodConnectAPI.Service.Interfaces.IDonationLabReportService, BloodConnectAPI.Service.Implementations.DonationLabReportService>();
builder.Services.AddScoped<BloodConnectAPI.Service.Interfaces.IPatientService, BloodConnectAPI.Service.Implementations.PatientService>();
builder.Services.AddScoped<BloodConnectAPI.Service.Interfaces.IReportService, BloodConnectAPI.Service.Implementations.ReportService>();
builder.Services.AddScoped<BloodConnectAPI.Service.Interfaces.INotificationService, BloodConnectAPI.Service.Implementations.NotificationService>();

var app = builder.Build();







// Seed Data

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // تطبيق Migrations تلقائياً
        logger.LogInformation("تطبيق migrations على قاعدة البيانات...");
        await context.Database.MigrateAsync();

        // تطبيق البيانات الأولية
     
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await DataSeeder.SeedAsync(context, userManager, roleManager);

        logger.LogInformation("✓ تم تطبيق البيانات الأولية بنجاح");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "✗ حدث خطأ أثناء تطبيق البيانات الأولية");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "BloodConnect API";
        options.Theme = Scalar.AspNetCore.ScalarTheme.Mars;
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");   // ← يجب أن يكون قبل UseStaticFiles
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html"); // دعم SPA
app.Run();


