using DataPrivacyAuditTool.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace DataPrivacyAuditTool.Tests.Unit.Services
{
    public class FileValidationServiceTests
    {
        private readonly string _validSettingsPath = Path.Combine("MockData", "valid_settings.json");
        private readonly string _missingFieldsPath = Path.Combine("MockData", "missing_fields_settings.json");
        private readonly string _malformedPath = Path.Combine("MockData", "malformed_settings.json");

        [Fact]
        public async Task ValidateFile_WithValidJson_ReturnsTrue()
        {
            // Arrange
            var fileValidationService = new FileValidationService();
            var fileContent = await File.ReadAllTextAsync(_validSettingsPath);
            var mockFile = CreateMockFile("Settings.json", fileContent); 

            // Act
            var result = await fileValidationService.ValidateSettingsFileAsync(mockFile.Object);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateFile_WithMalformedJson_ReturnsFalse()
        {
            // Arrange
            var fileValidationService = new FileValidationService();
            var malformedJson = "{ This is not valid JSON";
            var mockFile = CreateMockFile("Settings.json", malformedJson);

            // Act
            var result = await fileValidationService.ValidateSettingsFileAsync(mockFile.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateFile_WithMissingRequiredSections_ReturnsFalse()
        {
            // Arrange
            var fileValidationService = new FileValidationService();
            var jsonWithMissingSections = "{ \"App Settings\": [], \"Preferences\": [] }";
            var mockFile = CreateMockFile("Settings.json", jsonWithMissingSections); 

            // Act
            var result = await fileValidationService.ValidateSettingsFileAsync(mockFile.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateFile_WithIncorrectDataTypes_ReturnsFalse()
        {
            // Arrange
            var fileValidationService = new FileValidationService();
            var fileContent = await File.ReadAllTextAsync(_malformedPath);
            var mockFile = CreateMockFile("Settings.json", fileContent); 

            // Act
            var result = await fileValidationService.ValidateSettingsFileAsync(mockFile.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateFile_WithMissingFields_ReturnsTrue()
        {
            // Arrange
            var fileValidationService = new FileValidationService();
            var fileContent = await File.ReadAllTextAsync(_missingFieldsPath);
            var mockFile = CreateMockFile("Settings.json", fileContent); 

            // Act
            var result = await fileValidationService.ValidateSettingsFileAsync(mockFile.Object);

            // Assert
            Assert.True(result);
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
