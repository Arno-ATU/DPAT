using DataPrivacyAuditTool.Core.Interfaces;
using DataPrivacyAuditTool.Infrastructure.Services.Analyzers;
using DataPrivacyAuditTool.Infrastructure.Services;
using DataPrivacyAuditTool.Data;
using DataPrivacyAuditTool.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Database Configuration
builder.Services.AddDbContext<DpatDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// File Handling Layer
builder.Services.AddScoped<IFileValidationService, FileValidationService>();
builder.Services.AddScoped<IJsonParsingService, JsonParsingService>();

// Analysis Layer
builder.Services.AddScoped<IAnalyzerEngine, AnalyzerEngine>();

// Register specialized analyzers
builder.Services.AddScoped<IMetricAnalyzer, SearchEnginePrivacyAnalyzer>();
builder.Services.AddScoped<IMetricAnalyzer, PersonalDataExposureAnalyzer>();
builder.Services.AddScoped<IMetricAnalyzer, CookiePrivacyAnalyzer>();
builder.Services.AddScoped<IMetricAnalyzer, NetworkPredictionAnalyzer>();
builder.Services.AddScoped<IMetricAnalyzer, ExtensionPrivacyAnalyzer>();

// Presentation Layer
builder.Services.AddScoped<IPrivacyDashboardService, PrivacyDashboardService>();
builder.Services.AddScoped<IAuditHistoryService, AuditHistoryService>();

var app = builder.Build();

// Database setup with error handling
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<DpatDbContext>();
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        // Log database setup error
        Console.WriteLine($"Database setup error: {ex.Message}");
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Configure static files - this is crucial for CSS/JS/images
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
