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
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            try
            {
                return JsonSerializer.Deserialize<SettingsData>(jsonContent, options);
            }
            catch (JsonException ex)
            {
                throw new FormatException("Failed to parse Settings.json file.", ex);
            }
        }


        //Json Parsing for Addresses and more.json file
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
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            try
            {
                return JsonSerializer.Deserialize<AddressData>(jsonContent, options);
            }
            catch (JsonException ex)
            {
                throw new FormatException("Failed to parse Addresses and more.json file.", ex);
            }
        }
    }
}
