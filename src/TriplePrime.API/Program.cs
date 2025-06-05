using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Repositories;
using TriplePrime.Data.Services;
using TriplePrime.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IO;
using TriplePrime.Data.Models;

var builder = WebApplication.CreateBuilder(args);

// Add CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(corsOrigins ?? Array.Empty<string>())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add services to the container
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
    );

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add this line to resolve DbContext to ApplicationDbContext
builder.Services.AddScoped<DbContext, ApplicationDbContext>();

// Configure Email Settings
var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>();
if (emailSettings == null)
{
    throw new InvalidOperationException("Email settings not found in configuration.");
}
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Ensure email templates directory exists
var emailTemplatesPath = Path.Combine(builder.Environment.WebRootPath, emailSettings.TemplatesPath);
if (!Directory.Exists(emailTemplatesPath))
{
    Directory.CreateDirectory(emailTemplatesPath);
}

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// Register repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register services
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<UserService>();

// Configure and register EmailService
builder.Services.AddScoped<IEmailService, EmailService>();

// Register PaymentEmailService as singleton and hosted service
builder.Services.AddSingleton<PaymentEmailService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<PaymentEmailService>());

builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<SavingsPlanService>();
builder.Services.AddScoped<MarketerService>();
//builder.Services.AddScoped<ReferralService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<LoggingService>();
builder.Services.AddScoped<ConfigurationService>();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddScoped<FoodPackService>(sp => 
    new FoodPackService(
        sp.GetRequiredService<IUnitOfWork>(),
        Path.Combine(builder.Environment.WebRootPath, "images"),
        builder.Configuration["AppSettings:ApiBaseUrl"]
    ));
builder.Services.AddScoped<DeliveryService>();
builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();

// Add HttpClient
builder.Services.AddHttpClient();

// Add background services
builder.Services.AddHostedService<PaymentReminderService>();

var app = builder.Build();

// Configure the HTTP request pipeline
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

// Enable static file serving
app.UseStaticFiles();

app.UseHttpsRedirection();

// Add CORS middleware before authentication and authorization
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<ApplicationRole>>();
    var roles = new Dictionary<string, string>
    {
        { "Customer", "Regular customer with basic access" },
        { "Admin", "Administrator with full system access" },
        { "Marketer", "Marketer with referral and commission capabilities" }
    };

    foreach (var role in roles)
    {
        if (!roleManager.RoleExistsAsync(role.Key).GetAwaiter().GetResult())
        {
            await roleManager.CreateAsync(new ApplicationRole 
            { 
                Name = role.Key,
                Description = role.Value,
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    // Seed default admin user
    var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
    var adminEmail = "admin@tripleprime.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "admin@tripleprime.com",
            Email = adminEmail,
            FirstName = "System",
            LastName = "Administrator",
            PhoneNumber = "+2348000000000",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(adminUser, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

app.Run();
