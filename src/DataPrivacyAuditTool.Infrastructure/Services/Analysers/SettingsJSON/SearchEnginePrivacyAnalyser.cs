
using DataPrivacyAuditTool.Core.Models;

namespace DataPrivacyAuditTool.Infrastructure.Services.Analysers
{
    public class SearchEnginePrivacyAnalyser : SettingsAnalyser
    {
        public override string CategoryName => "Search Engine Privacy";
        public override string Description => "Analyssing your search engine settings for privacy implications";

        private static readonly Dictionary<string, PrivacyLevel> SearchEnginePrivacyLevels = new Dictionary<string, PrivacyLevel>(StringComparer.OrdinalIgnoreCase)
        {
            // High Privacy Engines
            { "DuckDuckGo", PrivacyLevel.High },
            { "Brave", PrivacyLevel.High },
            { "Proton", PrivacyLevel.High },
            { "PrivacyWall", PrivacyLevel.High },
            { "Firefox", PrivacyLevel.High },

            // Medium Privacy Engines
            { "Qwant", PrivacyLevel.Medium },
            { "Ecosia", PrivacyLevel.Medium },
            { "Startpage", PrivacyLevel.Medium },

            // Low Privacy Engines
            { "Google", PrivacyLevel.Low },
            { "Bing", PrivacyLevel.Low },
            { "Yahoo", PrivacyLevel.Low },
            { "Edge", PrivacyLevel.Low }
        };

        private enum PrivacyLevel
        {
            Low,
            Medium,
            High
        }

        protected override Task<PrivacyMetricCategory> AnalyseSettingsAsync(SettingsData settingsData)
        {
            var category = new PrivacyMetricCategory
            {
                Name = CategoryName,
                Description = Description,
                Metrics = new List<PrivacyMetric>()
            };

            // Add default search engine metric
            category.Metrics.Add(AnalyseDefaultSearchEngine(settingsData));

            // Add search suggestions metric
            category.Metrics.Add(AnalyseSearchSuggestions(settingsData));

            return Task.FromResult(category);
        }

        private PrivacyMetric AnalyseDefaultSearchEngine(SettingsData settingsData)
        {
            // Find the default search engine GUID
            var defaultSearchGuid = settingsData.Preferences
                .FirstOrDefault(p => p.Name == "default_search_provider.synced_guid")?.Value
                ?.Replace("\"", "");

            // Find the default engine
            var defaultEngine = settingsData.SearchEngines
                .FirstOrDefault(se => se.SyncGuid == defaultSearchGuid);

            if (defaultEngine == null)
            {
                return new PrivacyMetric
                {
                    Name = "Default Search Engine",
                    Value = "Unknown",
                    RiskLevel = RiskLevel.Medium,
                    Description = "Unable to determine default search engine",
                    Recommendation = "Set a privacy-focused search engine as your default"
                };
            }

            // Determine privacy level
            var privacyLevel = SearchEnginePrivacyLevels.TryGetValue(defaultEngine.ShortName, out var level)
                ? level
                : PrivacyLevel.Medium;

            // Map privacy levels to risk levels and create appropriate recommendation
            var (riskLevel, recommendation) = privacyLevel switch
            {
                PrivacyLevel.High => (RiskLevel.Low, $"{defaultEngine.ShortName} is an excellent privacy-focused search engine. Great choice!"),
                PrivacyLevel.Medium => (RiskLevel.Medium, $"{defaultEngine.ShortName} offers moderate privacy protection. Consider a more privacy-focused alternative."),
                PrivacyLevel.Low => (RiskLevel.High, $"{defaultEngine.ShortName} has known privacy concerns. Consider switching to a privacy-focused search engine."),
                _ => (RiskLevel.Medium, "Review your search engine's privacy settings")
            };

            return new PrivacyMetric
            {
                Name = "Default Search Engine",
                Value = defaultEngine.ShortName,
                RiskLevel = riskLevel,
                Description = $"Your default search engine is {defaultEngine.ShortName}",
                Recommendation = recommendation
            };
        }

        private PrivacyMetric AnalyseSearchSuggestions(SettingsData settingsData)
        {
            bool suggestionsEnabled = false;

            // Check if any active search engine has suggestions enabled
            foreach (var engine in settingsData.SearchEngines)
            {
                if (!string.IsNullOrEmpty(engine.SuggestionsUrl))
                {
                    suggestionsEnabled = true;
                    break;
                }
            }

            RiskLevel riskLevel = suggestionsEnabled ? RiskLevel.High : RiskLevel.Low;
            string description = suggestionsEnabled
                ? "Search suggestions are enabled, which sends your keystrokes to search providers as you type"
                : "Search suggestions are disabled, which prevents sending your keystrokes to search providers";

            string recommendation = suggestionsEnabled
                ? "Consider disabling search suggestions to prevent sending your partial queries to search providers"
                : "Good practice! Keeping search suggestions disabled improves privacy";

            return new PrivacyMetric
            {
                Name = "Search Suggestions",
                Value = suggestionsEnabled ? "Enabled" : "Disabled",
                RiskLevel = riskLevel,
                Description = description,
                Recommendation = recommendation
            };
        }
    }
}
