using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using DataPrivacyAuditTool.Core.Interfaces;
using DataPrivacyAuditTool.Core.Models;

namespace DataPrivacyAuditTool.Infrastructure.Services
{
    public class JsonParsingService:IJsonParsingService
    {
        // Parsing for Settings.json file
        public async Task<SettingsData> ParseSettingsJsonAsync(IFormFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            var jsonContent = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = null, // Setting to null keeps original property casing
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            try
            {
                var result = JsonSerializer.Deserialize<SettingsData>(jsonContent, options);

                // Debug the result
                System.Diagnostics.Debug.WriteLine($"Parsed SettingsData: SearchEngines={result?.SearchEngines?.Count ?? 0}, Preferences={result?.Preferences?.Count ?? 0}");

                return result;
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"JSON Exception: {ex.Message}");
                throw new FormatException("Failed to parse Settings.json file.", ex);
            }
        }

        // Json Parsing for Addresses and more.json file
        public async Task<AddressData> ParseAddressesJsonAsync(IFormFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            var jsonContent = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = null, // Setting to null keeps original property casing
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            try
            {
                var result = JsonSerializer.Deserialize<AddressData>(jsonContent, options);

                // Debug the result
                System.Diagnostics.Debug.WriteLine($"Parsed AddressData: Autofill={result?.Autofill?.Count ?? 0}, Profiles={result?.AutofillProfile?.Count ?? 0}, Contacts={result?.ContactInfo?.Count ?? 0}");

                return result;
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"JSON Exception: {ex.Message}");
                throw new FormatException("Failed to parse Addresses and more.json file.", ex);
            }
        }
    }
}
