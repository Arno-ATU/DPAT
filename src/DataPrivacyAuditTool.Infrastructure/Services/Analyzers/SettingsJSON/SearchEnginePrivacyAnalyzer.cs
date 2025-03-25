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

        private static readonly HashSet<string> PrivacyFocusedEngines = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "DuckDuckGo", "Qwant", "Brave", "Firefox", "PrivacyWall", "Ecosia"
        };

        private static readonly HashSet<string> LowPrivacyEngines = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Google", "Bing", "Yahoo", "Opera", "Edge"
        };

        protected override Task<PrivacyMetricCategory> AnalyzeSettingsAsync(SettingsData settingsData)
        {
            var category = new PrivacyMetricCategory
            {
                Name = CategoryName,
                Description = Description,
                Metrics = new List<PrivacyMetric>()
            };

            // Find default search engine
            var defaultSearchEngineMetric = AnalyzeDefaultSearchEngine(settingsData);
            category.Metrics.Add(defaultSearchEngineMetric);

            // Analyze search suggestions
            var searchSuggestionsMetric = AnalyzeSearchSuggestions(settingsData);
            category.Metrics.Add(searchSuggestionsMetric);

            // Analyze privacy-focused engines
            var privacyEnginesMetric = AnalyzePrivacyEngines(settingsData);
            category.Metrics.Add(privacyEnginesMetric);

            return Task.FromResult(category);
        }

        private PrivacyMetric AnalyzeDefaultSearchEngine(SettingsData settingsData)
        {
            // Find the default search engine GUID
            var defaultSearchGuid = settingsData.Preferences
                .FirstOrDefault(p => p.Name == "default_search_provider.synced_guid")?.Value;

            if (string.IsNullOrEmpty(defaultSearchGuid))
            {
                return new PrivacyMetric
                {
                    Name = "Default Search Engine",
                    Value = "Unknown",
                    RiskLevel = RiskLevel.Medium,
                    Description = "Unable to determine default search engine",
                    Recommendation = "Always consider setting a privacy-focused search engine as your default"
                };
            }

            // Clean up the GUID (it might have quotes)
            defaultSearchGuid = defaultSearchGuid.Replace("\"", "");

            // Find the engine with this GUID
            var defaultEngine = settingsData.SearchEngines
                .FirstOrDefault(se => se.SyncGuid == defaultSearchGuid);

            if (defaultEngine == null)
            {
                return new PrivacyMetric
                {
                    Name = "Default Search Engine",
                    Value = "Unknown",
                    RiskLevel = RiskLevel.Medium,
                    Description = "Unable to find default search engine in settings",
                    Recommendation = "Always consider setting a privacy-focused search engine as your default"
                };
            }

            // Evaluate privacy level of this engine
            RiskLevel riskLevel;
            string recommendation;

            if (PrivacyFocusedEngines.Contains(defaultEngine.ShortName))
            {
                riskLevel = RiskLevel.Low;
                recommendation = "Your default search engine prioritizes privacy. Good choice!";
            }
            else if (LowPrivacyEngines.Contains(defaultEngine.ShortName))
            {
                riskLevel = RiskLevel.High;
                recommendation = "Consider switching to a privacy-focused search engine like DuckDuckGo, Brave, or Firefox";
            }
            else
            {
                riskLevel = RiskLevel.Medium;
                recommendation = "Consider researching the privacy policy of your search engine or switching to a known privacy-focused alternative";
            }

            return new PrivacyMetric
            {
                Name = "Default Search Engine",
                Value = defaultEngine.ShortName,
                RiskLevel = riskLevel,
                Description = $"Your default search engine is {defaultEngine.ShortName}",
                Recommendation = recommendation
            };
        }

        private PrivacyMetric AnalyzeSearchSuggestions(SettingsData settingsData)
        {
            // Most engines have suggestions_url if suggestions are enabled
            int enginesWithSuggestions = settingsData.SearchEngines
                .Count(se => !string.IsNullOrEmpty(se.SuggestionsUrl));

            var activeEngines = settingsData.SearchEngines
                .Where(se => se.IsActive == "ACTIVE_STATUS_TRUE")
                .ToList();

            int activeEnginesWithSuggestions = activeEngines
                .Count(se => !string.IsNullOrEmpty(se.SuggestionsUrl));

            RiskLevel riskLevel;
            string description;
            string recommendation;

            if (activeEnginesWithSuggestions == 0)
            {
                riskLevel = RiskLevel.Low;
                description = "Search suggestions appear to be disabled for active search engines";
                recommendation = "This is good for privacy as search suggestions send your typing to search providers";
            }
            else if (activeEnginesWithSuggestions < activeEngines.Count)
            {
                riskLevel = RiskLevel.Medium;
                description = $"Search suggestions are enabled for {activeEnginesWithSuggestions} out of {activeEngines.Count} active search engines";
                recommendation = "Consider disabling search suggestions for all engines to improve privacy";
            }
            else
            {
                riskLevel = RiskLevel.High;
                description = "Search suggestions are enabled for all active search engines";
                recommendation = "Search suggestions send your typing to search providers in real-time. Consider disabling them for better privacy";
            }

            return new PrivacyMetric
            {
                Name = "Search Suggestions",
                Value = $"{activeEnginesWithSuggestions}/{activeEngines.Count}",
                RiskLevel = riskLevel,
                Description = description,
                Recommendation = recommendation
            };
        }

        private PrivacyMetric AnalyzePrivacyEngines(SettingsData settingsData)
        {
            int privacyEngineCount = settingsData.SearchEngines
                .Count(se => PrivacyFocusedEngines.Contains(se.ShortName));

            int totalEngines = settingsData.SearchEngines.Count;

            RiskLevel riskLevel;
            string recommendation;

            if (privacyEngineCount == 0)
            {
                riskLevel = RiskLevel.High;
                recommendation = "Consider adding privacy-focused search engines like DuckDuckGo, Brave, or Qwant";
            }
            else if (privacyEngineCount >= 2)
            {
                riskLevel = RiskLevel.Low;
                recommendation = "You have multiple privacy-focused search engines configured. Great job!";
            }
            else
            {
                riskLevel = RiskLevel.Medium;
                recommendation = "Consider adding more privacy-focused search engines as alternatives";
            }

            return new PrivacyMetric
            {
                Name = "Privacy-Focused Search Engines",
                Value = $"{privacyEngineCount}/{totalEngines}",
                RiskLevel = riskLevel,
                Description = $"You have {privacyEngineCount} privacy-focused search engines configured out of {totalEngines} total engines",
                Recommendation = recommendation
            };
        }
    }
}
