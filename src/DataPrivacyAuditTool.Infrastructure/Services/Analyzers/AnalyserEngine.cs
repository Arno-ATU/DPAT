using DataPrivacyAuditTool.Core.Interfaces;
using DataPrivacyAuditTool.Core.Models;
using Microsoft.Extensions.Logging;

namespace DataPrivacyAuditTool.Infrastructure.Services
{
    /// <summary>
    /// Coordinates the execution of all registered analyzers and compiles their results.
    /// The engine determines which analyzers to run based on available data files and
    /// handles error recovery if individual analyzers fail.
    /// </summary>
    public class AnalyserEngine:IAnalyserEngine
    {
        private readonly IEnumerable<IMetricAnalyser> _analyzers;
        private readonly ILogger<AnalyserEngine> _logger;

        public AnalyserEngine(
            IEnumerable<IMetricAnalyser> analyzers,
            ILogger<AnalyserEngine> logger)
        {
            _analyzers = analyzers ?? throw new ArgumentNullException(nameof(analyzers));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes all applicable analyzers and compiles their results.
        /// </summary>
        /// <remarks>
        /// This is an asynchronous method that returns a Promise-like object (Task) containing 
        /// the final analysis result. This async approach allows the engine to run multiple 
        /// analyzers concurrently if needed and prevents blocking the application while analysis 
        /// is happening.
        /// </remarks>
        /// <param name="data">The parsed Google data to analyze</param>
        /// <returns>A task containing the complete privacy analysis result</returns>
        public async Task<PrivacyAnalysisResult> ExecuteAnalysisAsync(ParsedGoogleData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var categories = new List<PrivacyMetricCategory>();
            var availableFileTypes = GetAvailableFileTypes(data);

            _logger.LogInformation("Starting privacy analysis with available file types: {FileTypes}",
                string.Join(", ", availableFileTypes));

            // Only run analyzers that can be supported by available data
            foreach (var analyzer in _analyzers.Where(a => availableFileTypes.Contains(a.RequiredFileType)))
            {
                try
                {
                    _logger.LogInformation("Running analyzer: {AnalyzerName}", analyzer.CategoryName);
                    var category = await analyzer.AnalyzeAsync(data);
                    categories.Add(category);
                }
                catch (Exception ex)
                {
                    // Log error and continue with other analyzers
                    _logger.LogError(ex, "Error running analyzer {AnalyzerName}: {ErrorMessage}",
                        analyzer.CategoryName, ex.Message);
                }
            }

            return new PrivacyAnalysisResult
            {
                Categories = categories,
                AnalysisDate = DateTime.UtcNow,
                IsPartialAnalysis = !HasAllFileTypes(availableFileTypes),
                PartialAnalysisMessage = GetPartialAnalysisMessage(availableFileTypes)
            };
        }

        /// <summary>
        /// Determines which file types are available in the provided data.
        /// </summary>
        /// <remarks>
        /// This function examines the provided data and builds a list of available file types. 
        /// It checks if each potential data source (SettingsData and AddressesData) is present, 
        /// and if so, adds the corresponding file type to the list. This list is then used to 
        /// determine which analyzers can run based on their file type requirements, supporting the 
        /// partial analysis scenario where a user uploads only one of the two possible files.
        /// </remarks>
        /// <param name="data">The parsed Google data to check</param>
        /// <returns>A list of available file types</returns>
        private List<AnalyzerFileType> GetAvailableFileTypes(ParsedGoogleData data)
        {
            var availableTypes = new List<AnalyzerFileType>();

            if (data.SettingsData != null)
                availableTypes.Add(AnalyzerFileType.Settings);

            if (data.AddressesData != null)
                availableTypes.Add(AnalyzerFileType.Addresses);

            return availableTypes;
        }

        private bool HasAllFileTypes(List<AnalyzerFileType> availableTypes)
        {
            return availableTypes.Contains(AnalyzerFileType.Settings) &&
                   availableTypes.Contains(AnalyzerFileType.Addresses);
        }

        private string GetPartialAnalysisMessage(List<AnalyzerFileType> availableTypes)
        {
            if (!availableTypes.Contains(AnalyzerFileType.Settings) &&
                !availableTypes.Contains(AnalyzerFileType.Addresses))
                return "No files were uploaded. Please upload at least one file to perform analysis.";

            if (!availableTypes.Contains(AnalyzerFileType.Settings))
                return "Settings.json file is missing. Only personal data analysis was performed. Upload Settings.json for browser configuration analysis.";

            if (!availableTypes.Contains(AnalyzerFileType.Addresses))
                return "Addresses and more.json file is missing. Only browser settings analysis was performed. Upload Addresses and more.json for personal data analysis.";

            return null; // Both files present
        }
    }
}
