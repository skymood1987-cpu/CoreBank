using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MinCoreBank.Data;
using MinCoreBank.Repositories;
using MinCoreBank.Services;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add MVC services
builder.Services.AddControllersWithViews();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database connections
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SecureConnection")));

builder.Services.AddScoped<IDbConnection>(_ =>
    new SqlConnection(builder.Configuration.GetConnectionString("SecureConnection")));

// Register services
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGeneralLedgerRepository, GeneralLedgerRepository>();
builder.Services.AddScoped<IGlTransactionRepository, GlTransactionRepository>();
builder.Services.AddScoped<IGlTreeReportRepository, GlTreeReportRepository>();
builder.Services.AddScoped<IGlTreeReportService, GlTreeReportService>();
// Add to Program.cs after builder.Services.AddAuthorization();
// Add to Program.cs after builder.Services.AddAuthorization();

// Add logging
builder.Services.AddLogging();

// Register the PasswordExpiryCheckService
builder.Services.AddHostedService<PasswordExpiryCheckService>();

// Update UserService registration to include logger
builder.Services.AddScoped<IUserService>(provider =>
    new UserService(
        provider.GetRequiredService<IConfiguration>(),
        provider.GetRequiredService<IPasswordHasher>(),
        provider.GetRequiredService<ILogger<UserService>>()
    ));// Add to your existing Program.cs services

// Register new approval services
builder.Services.AddScoped<IDailyBranchApprovalRepository, DailyBranchApprovalRepository>();
builder.Services.AddScoped<IDailyBranchApprovalService, DailyBranchApprovalService>();
builder.Services.AddScoped<IBranchLockService, BranchLockService>(); builder.Services.AddScoped<IDailyBranchApprovalRepository, DailyBranchApprovalRepository>();
// Cookie Authentication Configuration
// Cookie Authentication Configuration
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "MinCoreBank.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Set to None for testing IIS
        options.Cookie.SameSite = SameSiteMode.Lax; // Changed from Strict
        options.LoginPath = "/AuthView/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;

        // Important for SPA applications
        options.Events.OnRedirectToLogin = context =>
        {
            // Don't redirect API calls
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            }
            // For non-API calls, redirect to login
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            // Don't redirect API calls
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = 403;
                return Task.CompletedTask;
            }
            // For non-API calls, redirect to access denied
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();
// Add this after builder.Services.AddAuthorization();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PasswordChangeOnly", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("LimitedAccess", "password-change-only"));
});

var app = builder.Build();
//app.UseMiddleware<TimeRestrictionMiddleware>();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Authentication & Authorization middleware
app.UseAuthentication(); // Must come before UseAuthorization
app.UseAuthorization();
app.UseMiddleware<TimeRestrictionMiddleware>(); // ← THIS MUST BE AFTER authentication

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=GlTransactionsView}/{action=Index}/{id?}");
});

app.MapControllers();
app.Run();