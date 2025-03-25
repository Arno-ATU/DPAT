using DataPrivacyAuditTool.Core.Models;

namespace DataPrivacyAuditTool.Core.Interfaces
{
    /// <summary>
    /// Defines a privacy metric analyzer component that processes Google data files.
    /// </summary>
    /// <remarks>
    /// Analyzers specify which file types they require through "RequiredFileType",
    /// allowing the engine to determine eligibility based on available files.
    /// Results are organized by "CategoryName" in the UI with contextual
    /// information from "Description"
    /// </remarks>
    public interface IMetricAnalyzer
    {

        AnalyzerFileType RequiredFileType { get; }

        string CategoryName { get; }

        string Description { get; }

        /// <summary>
        /// Analyzes the provided Google data and returns privacy metrics.
        /// </summary>
        /// <param name="data">Parsed Google data containing available file content.</param>
        /// <returns>A task that resolves to privacy metrics for the analyzed category.</returns>
        Task<PrivacyMetricCategory> AnalyzeAsync(ParsedGoogleData data);
    }


    /// Defines the types of files that can be analyzed by metric analyzers.
    public enum AnalyzerFileType
    {

        Settings,
        Addresses
    }
}
