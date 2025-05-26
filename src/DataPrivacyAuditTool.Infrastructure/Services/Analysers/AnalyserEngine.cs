using DataPrivacyAuditTool.Core.Interfaces;
using DataPrivacyAuditTool.Core.Models;
using Microsoft.Extensions.Logging;

namespace DataPrivacyAuditTool.Infrastructure.Services.Analysers
{
    /// <summary>
    /// Coordinates the execution of all registered analyzers and compiles their results.
    /// The engine determines which analysers to run based on available data files and
    /// handles error recovery if individual analyzers fail.
    /// </summary>
    public class AnalyserEngine : IAnalyserEngine
    {
        private readonly IEnumerable<IMetricAnalyser> _analysers;
        private readonly ILogger<AnalyserEngine> _logger;

        public AnalyserEngine(
            IEnumerable<IMetricAnalyser> analysers,
            ILogger<AnalyserEngine> logger)
        {
            _analysers = analysers ?? throw new ArgumentNullException(nameof(analysers));
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

            // Only run analysers that can be supported by available data
            foreach (var analyser in _analysers.Where(a => availableFileTypes.Contains(a.RequiredFileType)))
            {
                try
                {
                    _logger.LogInformation("Running analyser: {AnalyserName}", analyser.CategoryName);
                    var category = await analyser.AnalyseAsync(data);
                    categories.Add(category);
                }
                catch (Exception ex)
                {
                    // Log error and continue with other analyzers
                    _logger.LogError(ex, "Error running analyser {AnalyserName}: {ErrorMessage}",
                        analyser.CategoryName, ex.Message);
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
        private List<AnalyserFileType> GetAvailableFileTypes(ParsedGoogleData data)
        {
            var availableTypes = new List<AnalyserFileType>();

            if (data.SettingsData != null)
                availableTypes.Add(AnalyserFileType.Settings);

            if (data.AddressesData != null)
                availableTypes.Add(AnalyserFileType.Addresses);

            return availableTypes;
        }

        private bool HasAllFileTypes(List<AnalyserFileType> availableTypes)
        {
            return availableTypes.Contains(AnalyserFileType.Settings) &&
                   availableTypes.Contains(AnalyserFileType.Addresses);
        }

        private string GetPartialAnalysisMessage(List<AnalyserFileType> availableTypes)
        {
            if (!availableTypes.Contains(AnalyserFileType.Settings) &&
                !availableTypes.Contains(AnalyserFileType.Addresses))
                return "No files were uploaded. Please upload at least one file to perform analysis.";

            if (!availableTypes.Contains(AnalyserFileType.Settings))
                return "Settings.json file is missing. Only personal data analysis was performed. Upload Settings.json for browser configuration analysis.";

            if (!availableTypes.Contains(AnalyserFileType.Addresses))
                return "Addresses and more.json file is missing. Only browser settings analysis was performed. Upload Addresses and more.json for personal data analysis.";

            return null; // Both files present
        }
    }
}
