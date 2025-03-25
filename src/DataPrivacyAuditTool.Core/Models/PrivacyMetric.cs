namespace DataPrivacyAuditTool.Core.Models
{
    public enum RiskLevel
    {
        Low,
        Medium,
        High,
        Critical
    }
    // class represents an individual finding from an analyzer.
    public class PrivacyMetric
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public string Description { get; set; }
        public string Recommendation { get; set; }
    }
}
