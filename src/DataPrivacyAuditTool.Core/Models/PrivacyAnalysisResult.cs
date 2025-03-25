using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace DataPrivacyAuditTool.Core.Models
{
    // This class represents the complete analysis output
    public class PrivacyAnalysisResult
    {
        public double OverallScore { get; set; }
        public List<PrivacyMetricCategory> Categories { get; set; } = new List<PrivacyMetricCategory>();
        public DateTime AnalysisDate { get; set; }
        public bool IsPartialAnalysis { get; set; }
        public string PartialAnalysisMessage { get; set; }



        // GetRecomendations(): extracts all important recommendations
        // Filters to only include Medium or higher risk items
        // Sorts recommendations by priority

        // This method will be used on the dashboard to show the most critical actions a user should take, without them having to dig through
        // all the detailed findings.

        public List<PrivacyMetric> GetRecommendations()
        {
            var recommendations = new List<PrivacyMetric>();

            foreach (var category in Categories)
            {
                foreach (var metric in category.Metrics)
                {
                    // Only include metrics with a risk level of Medium or higher
                    if (metric.RiskLevel >= RiskLevel.Medium && !string.IsNullOrEmpty(metric.Recommendation))
                    {
                        recommendations.Add(metric);
                    }
                }
            }

            // Sort by risk level (highest first)
            recommendations.Sort((a, b) => b.RiskLevel.CompareTo(a.RiskLevel));

            return recommendations;
        }
    }
}
