// File: DataPrivacyAuditTool.Core/Models/PrivacyAnalysisResult.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataPrivacyAuditTool.Core.Models
{
    public class PrivacyAnalysisResult
    {
        public List<PrivacyMetricCategory> Categories { get; set; } = new List<PrivacyMetricCategory>();

        public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;

        public bool IsPartialAnalysis { get; set; } = false;

        public string PartialAnalysisMessage { get; set; }

        public double OverallScore { get; set; } = 0.0;

        public IEnumerable<PrivacyMetric> GetRecommendations()
        {
            return Categories?
                .SelectMany(c => c.Metrics ?? Enumerable.Empty<PrivacyMetric>())
                .Where(m => m != null &&
                       (m.RiskLevel == RiskLevel.High ||
                        m.RiskLevel == RiskLevel.Critical))
                .OrderBy(m => m.RiskLevel)
                .Take(5) ?? Enumerable.Empty<PrivacyMetric>();
        }
    }
}
