using System.IO;
using System.Threading.Tasks;
using DataPrivacyAuditTool.Core.Interfaces;
using DataPrivacyAuditTool.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DataPrivacyAuditTool.Tests.Unit.Services
{
    public class AddressFileValidationTests
    {
        private readonly string _validAddressesPath = Path.Combine("MockData", "valid_addresses.json");
        private readonly string _missingFieldsPath = Path.Combine("MockData", "missing_fields_addresses.json");
        private readonly string _malformedPath = Path.Combine("MockData", "malformed_addresses.json");

        [Fact]
        public async Task ValidateAddressesFile_WithValidJson_ReturnsTrue()
        {
            // Arrange
            var fileValidationService = new FileValidationService();
            var fileContent = await File.ReadAllTextAsync(_validAddressesPath);
            var mockFile = CreateMockFile("Addresses and more.json", fileContent);

            // Act
            var result = await fileValidationService.ValidateAddressesFileAsync(mockFile.Object);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateAddressesFile_WithMalformedJson_ReturnsFalse()
        {
            // Arrange
            var fileValidationService = new FileValidationService();
            var fileContent = await File.ReadAllTextAsync(_malformedPath);
            var mockFile = CreateMockFile("Addresses and more.json", fileContent);

            // Act
            var result = await fileValidationService.ValidateAddressesFileAsync(mockFile.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateAddressesFile_WithMissingRequiredSections_ReturnsFalse()
        {
            // Arrange
            var fileValidationService = new FileValidationService();
            var jsonWithMissingSections = "{ \"Autofill\": [] }"; // Missing other required sections
            var mockFile = CreateMockFile("Addresses and more.json", jsonWithMissingSections);

            // Act
            var result = await fileValidationService.ValidateAddressesFileAsync(mockFile.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateAddressesFile_WithMissingFields_ReturnsTrue()
        {
            // Arrange
            var fileValidationService = new FileValidationService();
            var fileContent = await File.ReadAllTextAsync(_missingFieldsPath);
            var mockFile = CreateMockFile("Addresses and more.json", fileContent);

            // Act
            var result = await fileValidationService.ValidateAddressesFileAsync(mockFile.Object);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateAddressesFile_WithEmptyArrays_ReturnsTrue()
        {
            // Arrange
            var fileValidationService = new FileValidationService();
            var emptyJson = "{ \"Autofill\": [], \"Autofill Profile\": [], \"Contact Info\": [] }";
            var mockFile = CreateMockFile("Addresses and more.json", emptyJson);

            // Act
            var result = await fileValidationService.ValidateAddressesFileAsync(mockFile.Object);

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
