using DataPrivacyAuditTool.Core.Interfaces;
using DataPrivacyAuditTool.Infrastructure.Services.Analyzers;
using DataPrivacyAuditTool.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add core MVC services for web application
builder.Services.AddControllersWithViews();

// File Handling Layer - Services for processing and validating uploaded files
builder.Services.AddScoped<IFileValidationService, FileValidationService>();
builder.Services.AddScoped<IJsonParsingService, JsonParsingService>();

// Analysis Layer - Core engine and services for privacy analysis
builder.Services.AddScoped<IAnalyzerEngine, AnalyzerEngine>();

// Register specialized metric analyzers
// These allow for modular and extensible privacy metric analysis
builder.Services.AddScoped<IMetricAnalyzer, SearchEnginePrivacyAnalyzer>();
builder.Services.AddScoped<IMetricAnalyzer, PersonalDataExposureAnalyzer>();
builder.Services.AddScoped<IMetricAnalyzer, CookiePrivacyAnalyzer>();
builder.Services.AddScoped<IMetricAnalyzer, NetworkPredictionAnalyzer>();

// Optional: Additional analyzers can to be added in the future:
// builder.Services.AddScoped<IMetricAnalyzer, AutofillAnalyzer>();
// builder.Services.AddScoped<IMetricAnalyzer, CookieAnalyzer>();

// Optional: Additional services to be added in the futire:
// builder.Services.AddScoped<IRiskAssessmentService, RiskAssessmentService>();
builder.Services.AddScoped<IPrivacyDashboardService, PrivacyDashboardService>();
// builder.Services.AddScoped<IReportGenerationService, ReportGenerationService>();
// builder.Services.AddScoped<IPrivacyMetricsRepository, PrivacyMetricsRepository>();

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    // Use custom error handling in production
    app.UseExceptionHandler("/Home/Error");

    // HTTP Strict Transport Security (HSTS)
    // Enforces secure (HTTPS) connections
    app.UseHsts();
}

// Redirect HTTP to HTTPS
app.UseHttpsRedirection();

// Enable routing for controllers
app.UseRouting();

// Enable authorization middleware
app.UseAuthorization();

// Map static assets for improved performance
app.MapStaticAssets();

// Configure default route
// This means the application will start at the Home controller's Index action
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Start the application
app.Run();
