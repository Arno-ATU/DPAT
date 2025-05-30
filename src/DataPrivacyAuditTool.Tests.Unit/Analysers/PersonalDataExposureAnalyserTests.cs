using DataPrivacyAuditTool.Core.Models;
using DataPrivacyAuditTool.Infrastructure.Services.Analysers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DataPrivacyAuditTool.Tests.Unit.Analyzers
{
    /// <summary>
    /// Tests for the PersonalDataExposureAnalyser to verify it correctly assesses 
    /// personal data exposure in the browser
    /// </summary>
    public class PersonalDataExposureAnalyserTests
    {
        private readonly PersonalDataExposureAnalyser _analyser;

        public PersonalDataExposureAnalyserTests()
        {
            _analyser = new PersonalDataExposureAnalyser();
        }

        [Fact]
        public async Task AnalyseAsync_EmailExposure_DetectsMultipleEmails()
        {
            // Arrange
            var parsedData = new ParsedGoogleData
            {
                AddressesData = new AddressData
                {
                    Autofill = new List<AutofillEntry>
                    {
                        new AutofillEntry
                        {
                            Name = "email",
                            Value = "test1@example.com"
                        },
                        new AutofillEntry
                        {
                            Name = "email",
                            Value = "test2@example.com"
                        }
                    },
                    AutofillProfile = new List<AutofillProfile>
                    {
                        new AutofillProfile
                        {
                            Email = "test3@example.com"
                        }
                    }
                }
            };

            // Act
            var result = await _analyser.AnalyseAsync(parsedData);

            // Assert
            Assert.NotNull(result);

            // Find the email exposure metric
            var emailMetric = result.Metrics.Find(m => m.Name == "Email Exposure");
            Assert.NotNull(emailMetric);
            Assert.Equal("0/3", emailMetric.Value);
            Assert.Equal(RiskLevel.Low, emailMetric.RiskLevel);
        }

        [Fact]
        public async Task AnalyseAsync_PhoneNumberExposure_DetectsStoredPhoneNumbers()
        {
            // Arrange
            var parsedData = new ParsedGoogleData
            {
                AddressesData = new AddressData
                {
                    Autofill = new List<AutofillEntry>
                    {
                        new AutofillEntry
                        {
                            Name = "phoneNumber",
                            Value = "+123456789"
                        }
                    },
                    AutofillProfile = new List<AutofillProfile>
                    {
                        new AutofillProfile
                        {
                            Phone = "+987654321"
                        }
                    }
                }
            };

            // Act
            var result = await _analyser.AnalyseAsync(parsedData);

            // Assert
            Assert.NotNull(result);

            // Find the phone exposure metric
            var phoneMetric = result.Metrics.Find(m => m.Name == "Phone Number Exposure");
            Assert.NotNull(phoneMetric);
            Assert.Equal(RiskLevel.Low, phoneMetric.RiskLevel);
        }

        [Fact]
        public async Task AnalyseAsync_AddressExposure_DetectsStoredAddresses()
        {
            // Arrange
            var parsedData = new ParsedGoogleData
            {
                AddressesData = new AddressData
                {
                    AutofillProfile = new List<AutofillProfile>
                    {
                        new AutofillProfile
                        {
                            StreetAddress = "123 Test Street"
                        }
                    }
                }
            };

            // Act
            var result = await _analyser.AnalyseAsync(parsedData);

            // Assert
            Assert.NotNull(result);

            // Find the address exposure metric
            var addressMetric = result.Metrics.Find(m => m.Name == "Physical Address Exposure");
            Assert.NotNull(addressMetric);
            Assert.Equal("1/1", addressMetric.Value);
            Assert.Equal(RiskLevel.Low, addressMetric.RiskLevel);
        }

        [Fact]
        public async Task AnalyseAsync_OverallExposure_CalculatesCorrectExposureLevel()
        {
            // Arrange
            var parsedData = new ParsedGoogleData
            {
                AddressesData = new AddressData
                {
                    Autofill = new List<AutofillEntry>
                    {
                        new AutofillEntry { Name = "email", Value = "test@example.com" },
                        new AutofillEntry { Name = "phoneNumber", Value = "+123456789" }
                    },
                    AutofillProfile = new List<AutofillProfile>
                    {
                        new AutofillProfile
                        {
                            FullName = "Test User",
                            Email = "test2@example.com",
                            Phone = "+987654321",
                            StreetAddress = "123 Test Street"
                        }
                    },
                    ContactInfo = new List<ContactInfo>
                    {
                        new ContactInfo
                        {
                            Name = "Contact Name",
                            Email = "contact@example.com"
                        }
                    }
                }
            };

            // Act
            var result = await _analyser.AnalyseAsync(parsedData);

            // Assert
            Assert.NotNull(result);

            // Find the overall exposure metric
            var overallMetric = result.Metrics.Find(m => m.Name == "Overall Personal Data Exposure");
            Assert.NotNull(overallMetric);
            Assert.Equal(RiskLevel.Medium, overallMetric.RiskLevel);
        }
    }
}
