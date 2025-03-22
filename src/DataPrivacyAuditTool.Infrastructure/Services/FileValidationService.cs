using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using DataPrivacyAuditTool.Core.Interfaces;

namespace DataPrivacyAuditTool.Infrastructure.Services
{
    public class FileValidationService:IFileValidationService
    {
        // Required main sections in a valid Settings.json file
        private readonly string[] _requiredSettingsSections = new[]
        {
            "App Settings",
            "App Lists",
            "Search Engines",
            "Apps",
            "Preferences",
            "Themes",
            "Priority Preferences",
            "Web Apps"
        };

        // Required main sections in a valid Addresses.json file
        private readonly string[] _requiredAddressesSections = new[]
        {
            "Autofill",
            "Autofill Profile",
            "Contact Info"
        };

        
        public async Task<bool> ValidateSettingsFileAsync(IFormFile file)
        {
            // Keep your existing implementation from ValidateFileAsync
            if (file == null || file.Length == 0)
                return false;

            try
            {
                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);
                var jsonContent = await reader.ReadToEndAsync();

                // Attempt to parse as JSON
                var document = JsonDocument.Parse(jsonContent);
                var root = document.RootElement;

                // Check if it's an object
                if (root.ValueKind != JsonValueKind.Object)
                    return false;

                // Check for required sections
                foreach (var section in _requiredSettingsSections)
                {
                    if (!root.TryGetProperty(section, out var property))
                        return false;

                    // Verify each section is an array
                    if (property.ValueKind != JsonValueKind.Array)
                        return false;
                }

                return true;
            }
            catch (JsonException)
            {
                // JSON parsing failed
                return false;
            }
            catch (Exception)
            {
                // Any other exception
                return false;
            }
        }

        // Method for validating addresses file
        public async Task<bool> ValidateAddressesFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            try
            {
                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);
                var jsonContent = await reader.ReadToEndAsync();

                // Attempt to parse as JSON
                var document = JsonDocument.Parse(jsonContent);
                var root = document.RootElement;

                // Check if it's an object
                if (root.ValueKind != JsonValueKind.Object)
                    return false;

                // Check for required sections
                foreach (var section in _requiredAddressesSections)
                {
                    if (!root.TryGetProperty(section, out var property))
                        return false;

                    // Verify each section is an array
                    if (property.ValueKind != JsonValueKind.Array)
                        return false;
                }

                return true;
            }
            catch (JsonException)
            {
                // JSON parsing failed
                return false;
            }
            catch (Exception)
            {
                // Any other exception
                return false;
            }
        }
    }
}
