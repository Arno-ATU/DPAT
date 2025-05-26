using DataPrivacyAuditTool.Core.Models;
using DataPrivacyAuditTool.Infrastructure.Services.Analysers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DataPrivacyAuditTool.Tests.Unit.Analysers
{
    /// <summary>
    /// Tests for the SearchEnginePrivacyAnalyser to verify it correctly analyzes 
    /// search engine settings for privacy implications
    /// </summary>
    public class SearchEnginePrivacyAnalyserTests
    {
        private readonly SearchEnginePrivacyAnalyser _analyser;

        public SearchEnginePrivacyAnalyserTests()
        {
            _analyser = new SearchEnginePrivacyAnalyser();
        }

        [Fact]
        public async Task AnalyseAsync_DefaultSearchEngine_IdentifiesPrivacyFocusedEngine()
        {
            // Arrange
            var parsedData = new ParsedGoogleData
            {
                SettingsData = new SettingsData
                {
                    SearchEngines = new List<SearchEngine>
                    {
                        new SearchEngine
                        {
                            ShortName = "DuckDuckGo",
                            SyncGuid = "test-guid-duck",
                            IsActive = "ACTIVE_STATUS_TRUE"
                        }
                    },
                    Preferences = new List<Preference>
                    {
                        new Preference
                        {
                            Name = "default_search_provider.synced_guid",
                            Value = "\"test-guid-duck\""
                        }
                    }
                }
            };

            // Act
            var result = await _analyser.AnalyseAsync(parsedData);

            // Assert
            Assert.NotNull(result);

            // Find the default search engine metric
            var defaultEngineMetric = result.Metrics.Find(m => m.Name == "Default Search Engine");
            Assert.NotNull(defaultEngineMetric);
            Assert.Equal("DuckDuckGo", defaultEngineMetric.Value);
            Assert.Equal(RiskLevel.Low, defaultEngineMetric.RiskLevel);
        }

        [Fact]
        public async Task AnalyseAsync_DefaultSearchEngine_IdentifiesLowPrivacyEngine()
        {
            // Arrange
            var parsedData = new ParsedGoogleData
            {
                SettingsData = new SettingsData
                {
                    SearchEngines = new List<SearchEngine>
                    {
                        new SearchEngine
                        {
                            ShortName = "Google",
                            SyncGuid = "test-guid-google",
                            IsActive = "ACTIVE_STATUS_TRUE"
                        }
                    },
                    Preferences = new List<Preference>
                    {
                        new Preference
                        {
                            Name = "default_search_provider.synced_guid",
                            Value = "\"test-guid-google\""
                        }
                    }
                }
            };

            // Act
            var result = await _analyser.AnalyseAsync(parsedData);

            // Assert
            Assert.NotNull(result);

            // Find the default search engine metric
            var defaultEngineMetric = result.Metrics.Find(m => m.Name == "Default Search Engine");
            Assert.NotNull(defaultEngineMetric);
            Assert.Equal("Google", defaultEngineMetric.Value);
            Assert.Equal(RiskLevel.High, defaultEngineMetric.RiskLevel);
        }

        [Fact]
        public async Task AnalyseAsync_SearchSuggestions_DetectsEnabledSuggestions()
        {
            // Arrange
            var parsedData = new ParsedGoogleData
            {
                SettingsData = new SettingsData
                {
                    SearchEngines = new List<SearchEngine>
                    {
                        new SearchEngine
                        {
                            ShortName = "Google",
                            SuggestionsUrl = "https://example.com/suggest",
                            IsActive = "ACTIVE_STATUS_TRUE"
                        }
                    }
                }
            };

            // Act
            var result = await _analyser.AnalyseAsync(parsedData);

            // Assert
            Assert.NotNull(result);

            // Find the search suggestions metric
            var suggestionsMetric = result.Metrics.Find(m => m.Name == "Search Suggestions");
            Assert.NotNull(suggestionsMetric);
            Assert.Equal(RiskLevel.High, suggestionsMetric.RiskLevel);
        }
    }
}
