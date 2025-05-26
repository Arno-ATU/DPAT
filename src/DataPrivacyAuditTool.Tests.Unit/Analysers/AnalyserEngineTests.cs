using DataPrivacyAuditTool.Core.Interfaces;
using DataPrivacyAuditTool.Core.Models;
using DataPrivacyAuditTool.Infrastructure.Services.Analysers;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataPrivacyAuditTool.Tests.Unit.Analysers
{
    /// <summary>
    /// Tests for the AnalyserEngine to verify correct analysis orchestration
    /// and handling of different data scenarios.
    /// </summary>
    public class AnalyserEngineTests
    {
        private readonly Mock<ILogger<AnalyserEngine>> _loggerMock;
        private readonly Mock<IMetricAnalyser> _settingsAnalyserMock;
        private readonly Mock<IMetricAnalyser> _addressesAnalyserMock;

        public AnalyserEngineTests()
        {
            _loggerMock = new Mock<ILogger<AnalyserEngine>>();

            // Create the mock analyzers in the constructor
            _settingsAnalyserMock = new Mock<IMetricAnalyser>();
            _settingsAnalyserMock.Setup(a => a.RequiredFileType).Returns(AnalyserFileType.Settings);
            _settingsAnalyserMock.Setup(a => a.CategoryName).Returns("Test Settings Analyser");
            _settingsAnalyserMock.Setup(a => a.AnalyseAsync(It.IsAny<ParsedGoogleData>()))
                .ReturnsAsync(new PrivacyMetricCategory { Name = "Settings Category" });

            _addressesAnalyserMock = new Mock<IMetricAnalyser>();
            _addressesAnalyserMock.Setup(a => a.RequiredFileType).Returns(AnalyserFileType.Addresses);
            _addressesAnalyserMock.Setup(a => a.CategoryName).Returns("Test Addresses Analyser");
            _addressesAnalyserMock.Setup(a => a.AnalyseAsync(It.IsAny<ParsedGoogleData>()))
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

            var analysers = new List<IMetricAnalyser>
            {
                _settingsAnalyserMock.Object,
                _addressesAnalyserMock.Object
            };

            var engine = new AnalyserEngine(analysers, _loggerMock.Object);

            // Act
            var result = await engine.ExecuteAnalysisAsync(parsedData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Categories.Count);
            Assert.False(result.IsPartialAnalysis);
            Assert.Null(result.PartialAnalysisMessage);

            _settingsAnalyserMock.Verify(a => a.AnalyseAsync(It.IsAny<ParsedGoogleData>()), Times.Once);
            _addressesAnalyserMock.Verify(a => a.AnalyseAsync(It.IsAny<ParsedGoogleData>()), Times.Once);
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

            var analysers = new List<IMetricAnalyser>
            {
                _settingsAnalyserMock.Object,
                _addressesAnalyserMock.Object
            };

            var engine = new AnalyserEngine(analysers, _loggerMock.Object);

            // Act
            var result = await engine.ExecuteAnalysisAsync(parsedData);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Categories);
            Assert.True(result.IsPartialAnalysis);
            Assert.Contains("Addresses", result.PartialAnalysisMessage);

            _settingsAnalyserMock.Verify(a => a.AnalyseAsync(It.IsAny<ParsedGoogleData>()), Times.Once);
            _addressesAnalyserMock.Verify(a => a.AnalyseAsync(It.IsAny<ParsedGoogleData>()), Times.Never);
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

            var analyzers = new List<IMetricAnalyser>
            {
                _settingsAnalyserMock.Object,
                _addressesAnalyserMock.Object
            };

            var engine = new AnalyserEngine(analyzers, _loggerMock.Object);

            // Act
            var result = await engine.ExecuteAnalysisAsync(parsedData);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Categories);
            Assert.True(result.IsPartialAnalysis);
            Assert.Contains("Settings", result.PartialAnalysisMessage);

            _settingsAnalyserMock.Verify(a => a.AnalyseAsync(It.IsAny<ParsedGoogleData>()), Times.Never);
            _addressesAnalyserMock.Verify(a => a.AnalyseAsync(It.IsAny<ParsedGoogleData>()), Times.Once);
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
            _settingsAnalyserMock.Setup(a => a.AnalyseAsync(It.IsAny<ParsedGoogleData>()))
                .ThrowsAsync(new Exception("Test exception"));

            var analyzers = new List<IMetricAnalyser>
            {
                _settingsAnalyserMock.Object,
                _addressesAnalyserMock.Object
            };

            var engine = new AnalyserEngine(analyzers, _loggerMock.Object);

            // Act
            var result = await engine.ExecuteAnalysisAsync(parsedData);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Categories);  // Only addresses analyzer succeeded

            _settingsAnalyserMock.Verify(a => a.AnalyseAsync(It.IsAny<ParsedGoogleData>()), Times.Once);
            _addressesAnalyserMock.Verify(a => a.AnalyseAsync(It.IsAny<ParsedGoogleData>()), Times.Once);
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
