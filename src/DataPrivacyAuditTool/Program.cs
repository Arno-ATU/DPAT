var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Uncomment services as you implement them

// File Handling Layer - Your first focus area
// builder.Services.AddScoped<IFileUploadService, FileUploadService>();
// builder.Services.AddScoped<IFileValidationService, FileValidationService>();
// builder.Services.AddScoped<IJsonParsingService, JsonParsingService>();

// Analysis Layer
// builder.Services.AddScoped<IAnalyzerEngine, AnalyzerEngine>();
// builder.Services.AddScoped<IRiskAssessmentService, RiskAssessmentService>();

// Register specialized analyzers
// builder.Services.AddScoped<IMetricAnalyzer, SearchEngineAnalyzer>();
// builder.Services.AddScoped<IMetricAnalyzer, AutofillAnalyzer>();
// builder.Services.AddScoped<IMetricAnalyzer, CookieAnalyzer>();

// Presentation Layer
// builder.Services.AddScoped<IPrivacyDashboardService, PrivacyDashboardService>();
// builder.Services.AddScoped<IReportGenerationService, ReportGenerationService>();

// Data Layer
// builder.Services.AddScoped<IPrivacyMetricsRepository, PrivacyMetricsRepository>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
