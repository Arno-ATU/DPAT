using DataPrivacyAuditTool.Core.Interfaces;
using DataPrivacyAuditTool.Infrastructure.Services.Analyzers;
using DataPrivacyAuditTool.Infrastructure.Services;
using DataPrivacyAuditTool.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure Entity Framework with SQLite
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

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
