using System.Collections.Generic;

namespace DataPrivacyAuditTool.Core.Models.ViewModels
{
    public class PrivacyDashboardViewModel
    {
        public PrivacyAnalysisResult AnalysisResult { get; set; }
        public List<ChartDataViewModel> CategoryScores { get; set; } = new List<ChartDataViewModel>();
        public List<RiskBreakdownViewModel> RiskBreakdown { get; set; } = new List<RiskBreakdownViewModel>();
        public GaugeChartViewModel OverallScore { get; set; }
        public List<RecommendationViewModel> TopRecommendations { get; set; } = new List<RecommendationViewModel>();
    }

    public class ChartDataViewModel
    {
        public string Label { get; set; }
        public double Value { get; set; }
        public string Color { get; set; }
    }

    public class RiskBreakdownViewModel
    {
        public string RiskLevel { get; set; }
        public int Count { get; set; }
        public string Color { get; set; }
    }

    public class GaugeChartViewModel
    {
        public double Value { get; set; }
        public string Color { get; set; }
    }

    public class RecommendationViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Recommendation { get; set; }
        public RiskLevel RiskLevel { get; set; }
    }
}
