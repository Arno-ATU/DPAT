using DataPrivacyAuditTool.Core.Interfaces;
using DataPrivacyAuditTool.Core.Models;
using DataPrivacyAuditTool.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataPrivacyAuditTool.Tests.Unit.Analyzers
{
    /// <summary>
    /// Tests for the AnalyzerEngine to verify correct analysis orchestration
    /// and handling of different data scenarios.
    /// </summary>
    public class AnalyzerEngineTests
    {
        private readonly Mock<ILogger<AnalyzerEngine>> _loggerMock;
        private readonly Mock<IMetricAnalyzer> _settingsAnalyzerMock;
        private readonly Mock<IMetricAnalyzer> _addressesAnalyzerMock;

        public AnalyzerEngineTests()
        {
            _loggerMock = new Mock<ILogger<AnalyzerEngine>>();

            // Create the mock analyzers in the constructor
            _settingsAnalyzerMock = new Mock<IMetricAnalyzer>();
            _settingsAnalyzerMock.Setup(a => a.RequiredFileType).Returns(AnalyzerFileType.Settings);
            _settingsAnalyzerMock.Setup(a => a.CategoryName).Returns("Test Settings Analyzer");
            _settingsAnalyzerMock.Setup(a => a.AnalyzeAsync(It.IsAny<ParsedGoogleData>()))
                .ReturnsAsync(new PrivacyMetricCategory { Name = "Settings Category" });

            _addressesAnalyzerMock = new Mock<IMetricAnalyzer>();
            _addressesAnalyzerMock.Setup(a => a.RequiredFileType).Returns(AnalyzerFileType.Addresses);
            _addressesAnalyzerMock.Setup(a => a.CategoryName).Returns("Test Addresses Analyzer");
            _addressesAnalyzerMock.Setup(a => a.AnalyzeAsync(It.IsAny<ParsedGoogleData>()))
                .ReturnsAsync(new PrivacyMetricCategory { Name = "Addresses Category" });
        }

        [Fact]
        public async Task ExecuteAnalysisAsync_WithBothFilesPresent_ReturnsFullAnalysis()
        {
            // Arrange
            var parsedData = new ParsedGoogleData
            {
                SettingsData = new SettingsData(),
                AddressesData = new AddressData()
            };

            var analyzers = new List<IMetricAnalyzer>
            {
                _settingsAnalyzerMock.Object,
                _addressesAnalyzerMock.Object
            };

            var engine = new AnalyzerEngine(analyzers, _loggerMock.Object);

            // Act
            var result = await engine.ExecuteAnalysisAsync(parsedData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Categories.Count);
            Assert.False(result.IsPartialAnalysis);
            Assert.Null(result.PartialAnalysisMessage);

            _settingsAnalyzerMock.Verify(a => a.AnalyzeAsync(It.IsAny<ParsedGoogleData>()), Times.Once);
            _addressesAnalyzerMock.Verify(a => a.AnalyzeAsync(It.IsAny<ParsedGoogleData>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAnalysisAsync_WithOnlySettingsData_ReturnsPartialAnalysis()
        {
            // Arrange
            var parsedData = new ParsedGoogleData
            {
                SettingsData = new SettingsData(),
                AddressesData = null
            };

            var analyzers = new List<IMetricAnalyzer>
            {
                _settingsAnalyzerMock.Object,
                _addressesAnalyzerMock.Object
            };

            var engine = new AnalyzerEngine(analyzers, _loggerMock.Object);

            // Act
            var result = await engine.ExecuteAnalysisAsync(parsedData);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Categories);
            Assert.True(result.IsPartialAnalysis);
            Assert.Contains("Addresses", result.PartialAnalysisMessage);

            _settingsAnalyzerMock.Verify(a => a.AnalyzeAsync(It.IsAny<ParsedGoogleData>()), Times.Once);
            _addressesAnalyzerMock.Verify(a => a.AnalyzeAsync(It.IsAny<ParsedGoogleData>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAnalysisAsync_WithOnlyAddressesData_ReturnsPartialAnalysis()
        {
            // Arrange
            var parsedData = new ParsedGoogleData
            {
                SettingsData = null,
                AddressesData = new AddressData()
            };

            var analyzers = new List<IMetricAnalyzer>
            {
                _settingsAnalyzerMock.Object,
                _addressesAnalyzerMock.Object
            };

            var engine = new AnalyzerEngine(analyzers, _loggerMock.Object);

            // Act
            var result = await engine.ExecuteAnalysisAsync(parsedData);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Categories);
            Assert.True(result.IsPartialAnalysis);
            Assert.Contains("Settings", result.PartialAnalysisMessage);

            _settingsAnalyzerMock.Verify(a => a.AnalyzeAsync(It.IsAny<ParsedGoogleData>()), Times.Never);
            _addressesAnalyzerMock.Verify(a => a.AnalyzeAsync(It.IsAny<ParsedGoogleData>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAnalysisAsync_WithAnalyzerException_HandlesErrorAndContinues()
        {
            // Arrange
            var parsedData = new ParsedGoogleData
            {
                SettingsData = new SettingsData(),
                AddressesData = new AddressData()
            };

            // Override the setup to make settings analyzer throw an exception
            _settingsAnalyzerMock.Setup(a => a.AnalyzeAsync(It.IsAny<ParsedGoogleData>()))
                .ThrowsAsync(new Exception("Test exception"));

            var analyzers = new List<IMetricAnalyzer>
            {
                _settingsAnalyzerMock.Object,
                _addressesAnalyzerMock.Object
            };

            var engine = new AnalyzerEngine(analyzers, _loggerMock.Object);

            // Act
            var result = await engine.ExecuteAnalysisAsync(parsedData);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Categories);  // Only addresses analyzer succeeded

            _settingsAnalyzerMock.Verify(a => a.AnalyzeAsync(It.IsAny<ParsedGoogleData>()), Times.Once);
            _addressesAnalyzerMock.Verify(a => a.AnalyzeAsync(It.IsAny<ParsedGoogleData>()), Times.Once);
            _loggerMock.Verify(l => l.Log(
                It.Is<LogLevel>(ll => ll == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Test exception")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
