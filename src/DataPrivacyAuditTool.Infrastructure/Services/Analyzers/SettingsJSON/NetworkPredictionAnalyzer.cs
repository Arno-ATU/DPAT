using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataPrivacyAuditTool.Core.Models;

namespace DataPrivacyAuditTool.Infrastructure.Services.Analyzers
{
    public class NetworkPredictionAnalyzer : SettingsAnalyzer
    {
        public override string CategoryName => "Browser Network Privacy";
        public override string Description => "Analyzes how your browser's network settings may affect privacy";

        protected override Task<PrivacyMetricCategory> AnalyzeSettingsAsync(SettingsData settingsData)
        {
            var category = new PrivacyMetricCategory
            {
                Name = CategoryName,
                Description = Description,
                Metrics = new List<PrivacyMetric>()
            };

            // Add the network prediction metric
            var predictionMetric = AnalyzeNetworkPrediction(settingsData);
            category.Metrics.Add(predictionMetric);

            // We could add more network-related metrics here in the future
            // Such as DNS prefetching, WebRTC IP leakage settings, etc.

            return Task.FromResult(category);
        }

        private PrivacyMetric AnalyzeNetworkPrediction(SettingsData settingsData)
        {
            // Find the network prediction setting from preferences
            var predictionSetting = settingsData.Preferences
                .FirstOrDefault(p => p.Name == "net.network_prediction_options")?.Value;

            // Default value if not found (Google defaults to "1")
            string settingValue = predictionSetting ?? "1";

            // Network prediction values:
            // 0 = Always predict network actions (most privacy exposure)
            // 1 = Only predict on wifi networks (default)
            // 2 = Never predict network actions (best for privacy)

            int predictionLevel;
            if (!int.TryParse(settingValue, out predictionLevel))
            {
                predictionLevel = 1; // Default if we can't parse the value
            }

            string predictionStatus;
            RiskLevel riskLevel;
            string recommendation;

            switch (predictionLevel)
            {
                case 0:
                    predictionStatus = "Always enabled";
                    riskLevel = RiskLevel.High;
                    recommendation = "Network prediction is fully enabled (even on mobile networks), which sends additional browsing data to servers. Consider disabling this feature to reduce data exposure.";
                    break;

                case 1:
                    predictionStatus = "Enabled on WiFi";
                    riskLevel = RiskLevel.Medium;
                    recommendation = "Network prediction is enabled on WiFi networks, which may send additional browsing data to servers. For enhanced privacy, consider disabling this feature.";
                    break;

                case 2:
                    predictionStatus = "Disabled";
                    riskLevel = RiskLevel.Low;
                    recommendation = "Network prediction is disabled, which helps prevent additional data from being sent to servers. This is good for privacy.";
                    break;

                default:
                    predictionStatus = "Unknown";
                    riskLevel = RiskLevel.Medium;
                    recommendation = "Network prediction setting has an unrecognized value. For enhanced privacy, consider setting it to 2 (disabled).";
                    break;
            }

            string description = $"Network prediction is {predictionStatus.ToLower()}. " +
                                "This feature preloads web pages you might visit, which can send data to websites you haven't explicitly navigated to.";

            return new PrivacyMetric
            {
                Name = "Network Prediction",
                Value = predictionStatus,
                RiskLevel = riskLevel,
                Description = description,
                Recommendation = recommendation
            };
        }
    }
}
