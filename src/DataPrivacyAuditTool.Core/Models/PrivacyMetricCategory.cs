using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection.Emit;


// Groups related metrics together. Each analyzer returns one PrivacyMetricCategory that contains multiple PrivacyMetric objects.
// This establishes a hierarchy:
// PrivacyAnalysisResult(top level)
// Each contains multiple PrivacyMetrics (individual findings)

namespace DataPrivacyAuditTool.Core.Models
{
    public class PrivacyMetricCategory
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<PrivacyMetric> Metrics { get; set; } = new List<PrivacyMetric>();
        public double CategoryScore { get; set; }
    }
}
