using DataPrivacyAuditTool.Core.Models;
using DataPrivacyAuditTool.Infrastructure.Services.Analyzers;
using System.Text.Json;

public class CookiePrivacyAnalyser : SettingsAnalyzer
{
    public override string CategoryName => "Cookie Privacy";
    public override string Description => "Analyzes your cookie settings and exceptions for privacy implications";

    protected override Task<PrivacyMetricCategory> AnalyzeSettingsAsync(SettingsData settingsData)
    {
        var category = new PrivacyMetricCategory
        {
            Name = CategoryName,
            Description = Description,
            Metrics = new List<PrivacyMetric>()
        };

        // Add the cookie exceptions metric
        var cookieExceptionsMetric = AnalyzeCookieExceptions(settingsData);
        category.Metrics.Add(cookieExceptionsMetric);


        return Task.FromResult(category);
    }

    private PrivacyMetric AnalyzeCookieExceptions(SettingsData settingsData)
    {
        // Find the cookie exceptions from preferences
        string? cookieExceptionsValue = settingsData.Preferences
            .FirstOrDefault(p => p.Name == "profile.content_settings.exceptions.cookies")?.Value;

        int exceptionCount = 0;

        if (!string.IsNullOrEmpty(cookieExceptionsValue) && cookieExceptionsValue != "{}")
        {
            try
            {
                // Parse the JSON value (it's a JSON object stored as a string)
                // Format is typically: {"https://[*.]example.com:443,*":{"setting":1},...}
                var jsonObj = JsonDocument.Parse(cookieExceptionsValue);

                // Count the properties in the root object - each is a site exception
                exceptionCount = jsonObj.RootElement.EnumerateObject().Count();
            }
            catch (JsonException)
            {
                // If we can't parse the JSON, default to 0 exceptions
                exceptionCount = 0;
            }
        }

        // Determine risk level based on number of exceptions
        RiskLevel riskLevel;
        string recommendation;

        if (exceptionCount == 0)
        {
            riskLevel = RiskLevel.Low;
            recommendation = "Your browser has no cookie exceptions. This provides consistent privacy protection across all sites.";
        }
        else if (exceptionCount <= 5)
        {
            riskLevel = RiskLevel.Medium;
            recommendation = "You have a few cookie exceptions. Consider reviewing these and removing any that aren't essential.";
        }
        else if (exceptionCount <= 10)
        {
            riskLevel = RiskLevel.High;
            recommendation = "You have many cookie exceptions. These exceptions allow websites to track you despite your default cookie settings.";
        }
        else
        {
            riskLevel = RiskLevel.Critical;
            recommendation = "You have a large number of cookie exceptions. This significantly increases tracking risk and could compromise your privacy.";
        }

        return new PrivacyMetric
        {
            Name = "Cookie Exceptions",
            Value = exceptionCount.ToString(),
            RiskLevel = riskLevel,
            Description = $"Found {exceptionCount} websites with special cookie permissions that override your default settings",
            Recommendation = recommendation
        };
    }
}
