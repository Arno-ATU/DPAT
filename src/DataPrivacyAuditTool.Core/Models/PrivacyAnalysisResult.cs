using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DataPrivacyAuditTool.Core.Models
{
    /// <summary>
    /// Represents the complete result of a privacy analysis
    /// </summary>
    public class PrivacyAnalysisResult
    {
        /// <summary>
        /// List of privacy metric categories analyzed
        /// </summary>
        [JsonPropertyName("categories")]
        public List<PrivacyMetricCategory> Categories { get; set; } = new List<PrivacyMetricCategory>();

        /// <summary>
        /// Date and time when the analysis was performed
        /// </summary>
        [JsonPropertyName("analysisDate")]
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Indicates if the analysis is incomplete due to missing files
        /// </summary>
        [JsonPropertyName("isPartialAnalysis")]
        public bool IsPartialAnalysis { get; set; }

        /// <summary>
        /// Message explaining why the analysis is partial
        /// </summary>
        [JsonPropertyName("partialAnalysisMessage")]
        public string PartialAnalysisMessage { get; set; }
    }
}
