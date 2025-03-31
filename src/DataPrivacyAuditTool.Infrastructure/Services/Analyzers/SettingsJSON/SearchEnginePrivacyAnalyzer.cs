using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataPrivacyAuditTool.Core.Models;

namespace DataPrivacyAuditTool.Infrastructure.Services.Analyzers
{
    public class SearchEnginePrivacyAnalyzer:SettingsAnalyzer
    {
        public override string CategoryName => "Search Engine Privacy";
        public override string Description => "Analyzes your search engine settings for privacy implications";

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

        protected override Task<PrivacyMetricCategory> AnalyzeSettingsAsync(SettingsData settingsData)
        {
            var category = new PrivacyMetricCategory
            {
                Name = CategoryName,
                Description = Description,
                Metrics = new List<PrivacyMetric>()
            };

            // Add default search engine metric
            category.Metrics.Add(AnalyzeDefaultSearchEngine(settingsData));


            return Task.FromResult(category);
        }

        private PrivacyMetric AnalyzeDefaultSearchEngine(SettingsData settingsData)
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

        
    }
}
