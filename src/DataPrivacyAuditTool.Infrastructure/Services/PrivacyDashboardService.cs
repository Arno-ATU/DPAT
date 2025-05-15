using DataPrivacyAuditTool.Core.Interfaces;
using DataPrivacyAuditTool.Core.Models;
using DataPrivacyAuditTool.Core.Models.ViewModels;

namespace DataPrivacyAuditTool.Infrastructure.Services
{
    public class PrivacyDashboardService:IPrivacyDashboardService
    {
        public PrivacyDashboardViewModel PrepareDashboardData(PrivacyAnalysisResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            var viewModel = new PrivacyDashboardViewModel
            {
                AnalysisResult = result,
                CategoryScores = PrepareCategoryScores(result),
                RiskBreakdown = PrepareRiskBreakdown(result),
                OverallScore = PrepareOverallScore(result),
                TopRecommendations = PrepareTopRecommendations(result)
            };

            return viewModel;
        }

        private List<ChartDataViewModel> PrepareCategoryScores(PrivacyAnalysisResult result)
        {
            // Calculate scores for each category
            return result.Categories.Select(category =>
            {
                // Calculate average score for the category based on risk levels
                double categoryScore = CalculateCategoryScore(category);

                return new ChartDataViewModel
                {
                    Label = category.Name,
                    Value = categoryScore,
                    Color = GetColorForScore(categoryScore)
                };
            }).ToList();
        }

        private double CalculateCategoryScore(PrivacyMetricCategory category)
        {
            if (category.Metrics == null || !category.Metrics.Any())
                return 0;

            // Convert risk levels to scores (higher is better)
            var scores = category.Metrics.Select(m => RiskLevelToScore(m.RiskLevel));

            // Calculate average and normalize to 0-100
            return scores.Average() * 100 / 3; // 3 is the max score (Low risk)
        }

        private int RiskLevelToScore(RiskLevel level)
        {
            return level switch
            {
                RiskLevel.Low => 3,
                RiskLevel.Medium => 2,
                RiskLevel.High => 1,
                RiskLevel.Critical => 0,
                _ => 0
            };
        }

        private string GetColorForScore(double score)
        {
            return score switch
            {
                double s when s >= 80 => "#28a745", // Green
                double s when s >= 60 => "#5cb85c", // Light green
                double s when s >= 40 => "#ffc107", // Yellow
                double s when s >= 20 => "#fd7e14", // Orange
                _ => "#dc3545"                      // Red
            };
        }

        private List<RiskBreakdownViewModel> PrepareRiskBreakdown(PrivacyAnalysisResult result)
        {
            var allMetrics = result.Categories.SelectMany(c => c.Metrics).ToList();

            return new List<RiskBreakdownViewModel>
            {
                new RiskBreakdownViewModel
                {
                    RiskLevel = "Low",
                    Count = allMetrics.Count(m => m.RiskLevel == RiskLevel.Low),
                    Color = "#28a745" // Green
                },
                new RiskBreakdownViewModel
                {
                    RiskLevel = "Medium",
                    Count = allMetrics.Count(m => m.RiskLevel == RiskLevel.Medium),
                    Color = "#ffc107" // Yellow
                },
                new RiskBreakdownViewModel
                {
                    RiskLevel = "High",
                    Count = allMetrics.Count(m => m.RiskLevel == RiskLevel.High),
                    Color = "#fd7e14" // Orange
                },
                new RiskBreakdownViewModel
                {
                    RiskLevel = "Critical",
                    Count = allMetrics.Count(m => m.RiskLevel == RiskLevel.Critical),
                    Color = "#dc3545" // Red
                }
            };
        }

        private GaugeChartViewModel PrepareOverallScore(PrivacyAnalysisResult result)
        {
            // Calculate overall score as an average of category scores
            double overallScore = 0;

            if (result.Categories != null && result.Categories.Any())
            {
                var categoryScores = result.Categories
                    .Select(CalculateCategoryScore)
                    .ToList();

                overallScore = categoryScores.Average();
            }

            return new GaugeChartViewModel
            {
                Value = Math.Round(overallScore, 1),
                Color = GetColorForScore(overallScore)
            };
        }

        private List<RecommendationViewModel> PrepareTopRecommendations(PrivacyAnalysisResult result)
        {
            // Get recommendations from high and critical risk metrics
            return result.GetRecommendations()
                .Select(m => new RecommendationViewModel
                {
                    Name = m.Name,
                    Description = m.Description,
                    Recommendation = m.Recommendation,
                    RiskLevel = m.RiskLevel
                })
                .ToList();
        }
    }
}
