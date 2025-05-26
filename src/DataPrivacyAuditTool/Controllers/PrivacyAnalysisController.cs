using System.Threading.Tasks;
using DataPrivacyAuditTool.Core.Interfaces;
using DataPrivacyAuditTool.Core.Models;
using DataPrivacyAuditTool.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DataPrivacyAuditTool.Controllers
{
    public class PrivacyAnalysisController : Controller
    {
        private readonly IFileValidationService _fileValidationService;
        private readonly IJsonParsingService _jsonParsingService;
        private readonly IAnalyserEngine _analyzerEngine;
        private readonly IPrivacyDashboardService _dashboardService;
        private readonly IAuditHistoryService _auditHistoryService;

        public PrivacyAnalysisController(
            IFileValidationService fileValidationService,
            IJsonParsingService jsonParsingService,
            IAnalyserEngine analyzerEngine,
            IPrivacyDashboardService dashboardService,
            IAuditHistoryService auditHistoryService)
        {
            _fileValidationService = fileValidationService;
            _jsonParsingService = jsonParsingService;
            _analyzerEngine = analyzerEngine;
            _dashboardService = dashboardService;
            _auditHistoryService = auditHistoryService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile settingsFile, IFormFile addressesFile, string username)
        {
            // Debug logging
            Console.WriteLine($"=== Upload Debug ===");
            Console.WriteLine($"Username received: '{username}'");
            Console.WriteLine($"Settings file: {settingsFile?.FileName ?? "null"}");
            Console.WriteLine($"Addresses file: {addressesFile?.FileName ?? "null"}");

            // Create a container for our parsed data
            var parsedData = new ParsedGoogleData();

            // Try to parse the settings file if provided
            if (settingsFile != null)
            {
                bool isValidSettings = await _fileValidationService.ValidateSettingsFileAsync(settingsFile);
                if (isValidSettings)
                {
                    parsedData.SettingsData = await _jsonParsingService.ParseSettingsJsonAsync(settingsFile);
                }
                else
                {
                    ModelState.AddModelError("settingsFile", "The Settings.json file is not valid");
                }
            }

            // Try to parse the addresses file if provided
            if (addressesFile != null)
            {
                bool isValidAddresses = await _fileValidationService.ValidateAddressesFileAsync(addressesFile);
                if (isValidAddresses)
                {
                    parsedData.AddressesData = await _jsonParsingService.ParseAddressesJsonAsync(addressesFile);
                }
                else
                {
                    ModelState.AddModelError("addressesFile", "The Addresses and more.json file is not valid");
                }
            }

            // Check if we have at least one valid file
            if (parsedData.SettingsData == null && parsedData.AddressesData == null)
            {
                ModelState.AddModelError("", "Please upload at least one valid file to analyze");
                return View("Index");
            }

            // Run the analysis
            var analysisResult = await _analyzerEngine.ExecuteAnalysisAsync(parsedData);

            // CALCULATE THE OVERALL SCORE BEFORE SAVING
            // The dashboard service calculates the overall score, so this needs to be done first
            var dashboardData = _dashboardService.PrepareDashboardData(analysisResult);

            // Update the analysis result with the calculated overall score
            analysisResult.OverallScore = dashboardData.OverallScore.Value;

            // Debug: Log analysis result
            Console.WriteLine($"Analysis completed. Score: {analysisResult.OverallScore}");
            Console.WriteLine($"Analysis date: {analysisResult.AnalysisDate}");

            // Save audit to database if username provided
            if (!string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine($"Attempting to save audit for username: '{username}'");
                try
                {
                    var savedId = await _auditHistoryService.SaveAuditAsync(username, analysisResult);
                    Console.WriteLine($"Successfully saved audit with ID: {savedId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR saving audit: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
            else
            {
                Console.WriteLine("No username provided - skipping database save");
            }

            // Store the result in TempData to persist across redirects
            TempData["AnalysisResult"] = JsonSerializer.Serialize(analysisResult);

            return RedirectToAction("Results");
        }

        public IActionResult Results()
        {
            // Retrieve the analysis result from TempData
            if (TempData["AnalysisResult"] is string serializedResult)
            {
                var analysisResult = JsonSerializer.Deserialize<PrivacyAnalysisResult>(serializedResult);

                // Prepare dashboard data
                var dashboardData = _dashboardService.PrepareDashboardData(analysisResult);

                return View(dashboardData);
            }

            return RedirectToAction("Index");
        }
    }
}
