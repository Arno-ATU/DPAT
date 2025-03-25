using System.Threading.Tasks;
using DataPrivacyAuditTool.Core.Interfaces;
using DataPrivacyAuditTool.Core.Models; // Ensure this namespace is correct
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json; // Add this for JsonSerializer

namespace DataPrivacyAuditTool.Controllers
{
    public class PrivacyAnalysisController:Controller
    {
        private readonly IFileValidationService _fileValidationService;
        private readonly IJsonParsingService _jsonParsingService;
        private readonly IAnalyzerEngine _analyzerEngine;

        public PrivacyAnalysisController(
            IFileValidationService fileValidationService,
            IJsonParsingService jsonParsingService,
            IAnalyzerEngine analyzerEngine)
        {
            _fileValidationService = fileValidationService;
            _jsonParsingService = jsonParsingService;
            _analyzerEngine = analyzerEngine;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile settingsFile, IFormFile addressesFile)
        {
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

            // Store the result in TempData to persist across redirects
            TempData["AnalysisResult"] = System.Text.Json.JsonSerializer.Serialize(analysisResult);

            return RedirectToAction("Results");
        }

        public IActionResult Results()
        {
            // Retrieve the analysis result from TempData
            if (TempData["AnalysisResult"] is string serializedResult)
            {
                var analysisResult = System.Text.Json.JsonSerializer.Deserialize<PrivacyAnalysisResult>(serializedResult);
                return View(analysisResult);
            }

            return RedirectToAction("Index");
        }
    }
}
