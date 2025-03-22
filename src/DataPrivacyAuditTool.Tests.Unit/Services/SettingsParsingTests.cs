using System;
using System.IO;
using System.Threading.Tasks;
using DataPrivacyAuditTool.Core.Interfaces;
using DataPrivacyAuditTool.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DataPrivacyAuditTool.Tests.Unit.Services
{
    public class SettingsParsingTests
    {
        private readonly string _validSettingsPath = Path.Combine("MockData", "valid_settings.json");
        private readonly string _malformedSettingsPath = Path.Combine("MockData", "malformed_settings.json");
        private readonly JsonParsingService _parsingService;

        public SettingsParsingTests()
        {
            _parsingService = new JsonParsingService();
        }

        [Fact]
        public async Task ParseSettingsJsonAsync_WithValidJson_ReturnsSettingsData()
        {
            // Arrange
            var fileContent = await File.ReadAllTextAsync(_validSettingsPath);
            var mockFile = CreateMockFile("Settings.json", fileContent);

            // Act
            var result = await _parsingService.ParseSettingsJsonAsync(mockFile.Object);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.SearchEngines);
            Assert.NotNull(result.Preferences);

            // Debug statements to help identify issues
            System.Diagnostics.Debug.WriteLine($"SearchEngines count: {result.SearchEngines?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"Preferences count: {result.Preferences?.Count ?? 0}");

            // These collections should have data in our valid mock file
            Assert.True(result.SearchEngines.Count > 0, "SearchEngines collection should not be empty");
            Assert.True(result.Preferences.Count > 0, "Preferences collection should not be empty");
        }

        [Fact]
        public async Task ParseSettingsJsonAsync_WithNullFile_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _parsingService.ParseSettingsJsonAsync(null));
        }

        [Fact]
        public async Task ParseSettingsJsonAsync_WithMalformedJson_ThrowsFormatException()
        {
            // Arrange
            var fileContent = await File.ReadAllTextAsync(_malformedSettingsPath);
            var mockFile = CreateMockFile("Settings.json", fileContent);

            // Act & Assert
            await Assert.ThrowsAsync<FormatException>(() =>
                _parsingService.ParseSettingsJsonAsync(mockFile.Object));
        }

        [Fact]
        public async Task ParseSettingsJsonAsync_ChecksSpecificDataMapping()
        {
            // Arrange
            var fileContent = await File.ReadAllTextAsync(_validSettingsPath);
            var mockFile = CreateMockFile("Settings.json", fileContent);

            // Act
            var result = await _parsingService.ParseSettingsJsonAsync(mockFile.Object);

            // Assert
            // Debug output to help identify issues
            System.Diagnostics.Debug.WriteLine($"SearchEngines count: {result.SearchEngines.Count}");
            foreach (var engine in result.SearchEngines)
            {
                System.Diagnostics.Debug.WriteLine($"Engine: {engine.ShortName}, URL: {engine.Url}");
            }

            // Find a search engine that we know exists in our mock data
            // Changed from DuckDuckGo to Google which is definitely in the mock data
            var googleEngine = result.SearchEngines.Find(se => se.ShortName == "Google");
            Assert.NotNull(googleEngine);
            Assert.Contains("google", googleEngine.Url.ToLower());

            // Find a preference that we know exists in our mock data
            var networkPref = result.Preferences.Find(p => p.Name == "net.network_prediction_options");
            Assert.NotNull(networkPref);
            Assert.NotNull(networkPref.Value);
        }

        private Mock<IFormFile> CreateMockFile(string fileName, string content)
        {
            var mock = new Mock<IFormFile>();
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            mock.Setup(f => f.FileName).Returns(fileName);
            mock.Setup(f => f.Length).Returns(ms.Length);
            mock.Setup(f => f.OpenReadStream()).Returns(ms);
            mock.Setup(f => f.ContentType).Returns("application/json");

            return mock;
        }
    }
}
