using DataPrivacyAuditTool.Core.Interfaces;
using DataPrivacyAuditTool.Core.Models;
using DataPrivacyAuditTool.Infrastructure.Services.Analyzers;

namespace DataPrivacyAuditTool.Tests.Unit.Analyzers
{
    /// <summary>
    /// Tests for the abstract base analyzer classes that verify their core behavior
    /// </summary>
    public class BaseAnalyzersTests
    {
        // Test implementations of abstract classes
        private class TestSettingsAnalyzer:SettingsAnalyzer
        {
            public override string CategoryName => "Test Settings Category";
            public override string Description => "Test Settings Description";

            protected override Task<PrivacyMetricCategory> AnalyzeSettingsAsync(SettingsData settingsData)
            {
                return Task.FromResult(new PrivacyMetricCategory
                {
                    Name = CategoryName,
                    Description = Description,
                    Metrics = new List<PrivacyMetric> { new PrivacyMetric { Name = "Test Metric" } }
                });
            }
        }

        private class TestAddressesAnalyzer:AddressesAnalyzer
        {
            public override string CategoryName => "Test Addresses Category";
            public override string Description => "Test Addresses Description";

            protected override Task<PrivacyMetricCategory> AnalyzeAddressesAsync(AddressData addressesData)
            {
                return Task.FromResult(new PrivacyMetricCategory
                {
                    Name = CategoryName,
                    Description = Description,
                    Metrics = new List<PrivacyMetric> { new PrivacyMetric { Name = "Test Metric" } }
                });
            }
        }

        [Fact]
        public async Task SettingsAnalyzer_CallsAnalyzeSettingsAsync_WhenDataIsProvided()
        {
            // Arrange
            var analyzer = new TestSettingsAnalyzer();
            var parsedData = new ParsedGoogleData
            {
                SettingsData = new SettingsData()
            };

            // Act
            var result = await analyzer.AnalyzeAsync(parsedData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Settings Category", result.Name);
            Assert.Single(result.Metrics);
            Assert.Equal("Test Metric", result.Metrics[0].Name);
        }

        [Fact]
        public async Task SettingsAnalyzer_ThrowsArgumentException_WhenSettingsDataIsNull()
        {
            // Arrange
            var analyzer = new TestSettingsAnalyzer();
            var parsedData = new ParsedGoogleData
            {
                SettingsData = null,
                AddressesData = new AddressData()
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => analyzer.AnalyzeAsync(parsedData));
        }

        [Fact]
        public async Task AddressesAnalyzer_CallsAnalyzeAddressesAsync_WhenDataIsProvided()
        {
            // Arrange
            var analyzer = new TestAddressesAnalyzer();
            var parsedData = new ParsedGoogleData
            {
                AddressesData = new AddressData()
            };

            // Act
            var result = await analyzer.AnalyzeAsync(parsedData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Addresses Category", result.Name);
            Assert.Single(result.Metrics);
            Assert.Equal("Test Metric", result.Metrics[0].Name);
        }

        [Fact]
        public async Task AddressesAnalyzer_ThrowsArgumentException_WhenAddressesDataIsNull()
        {
            // Arrange
            var analyzer = new TestAddressesAnalyzer();
            var parsedData = new ParsedGoogleData
            {
                SettingsData = new SettingsData(),
                AddressesData = null
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => analyzer.AnalyzeAsync(parsedData));
        }
    }
}
