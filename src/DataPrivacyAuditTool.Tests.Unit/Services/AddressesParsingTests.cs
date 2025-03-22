using DataPrivacyAuditTool.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;

public class AddressParsingTests
{
    private readonly string _validAddressesPath = Path.Combine("MockData", "valid_addresses.json");
    private readonly JsonParsingService _parsingService;

    public AddressParsingTests()
    {
        _parsingService = new JsonParsingService();
    }

    [Fact]
    public async Task ParseAddressesJsonAsync_WithValidJson_ReturnsAddressData()
    {
        // Arrange
        var fileContent = await File.ReadAllTextAsync(_validAddressesPath);
        var mockFile = CreateMockFile("Addresses and more.json", fileContent);

        // Act
        var result = await _parsingService.ParseAddressesJsonAsync(mockFile.Object);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Autofill);
        Assert.NotNull(result.AutofillProfile);
        Assert.NotNull(result.ContactInfo);
    }

    [Fact]
    public async Task ParseAddressesJsonAsync_WithNullFile_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _parsingService.ParseAddressesJsonAsync(null));
    }

    [Fact]
    public async Task ParseAddressesJsonAsync_WithMalformedJson_ThrowsFormatException()
    {
        // Arrange
        var malformedJson = "{ This is not valid JSON";
        var mockFile = CreateMockFile("Addresses and more.json", malformedJson);

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() =>
            _parsingService.ParseAddressesJsonAsync(mockFile.Object));
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
