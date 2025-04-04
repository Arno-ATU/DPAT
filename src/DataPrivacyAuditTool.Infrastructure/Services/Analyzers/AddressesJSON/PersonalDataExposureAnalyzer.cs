using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataPrivacyAuditTool.Core.Models;

namespace DataPrivacyAuditTool.Infrastructure.Services.Analyzers
{
    public class PersonalDataExposureAnalyzer:AddressesAnalyzer
    {
        public override string CategoryName => "Personal Data Exposure";
        public override string Description => "Analyzes how much personal information is stored in autofill";

        protected override Task<PrivacyMetricCategory> AnalyzeAddressesAsync(AddressData addressesData)
        {
            var category = new PrivacyMetricCategory
            {
                Name = CategoryName,
                Description = Description,
                Metrics = new List<PrivacyMetric>()
            };

            // Analyze email exposure
            var emailMetric = AnalyzeEmailExposure(addressesData);
            category.Metrics.Add(emailMetric);

            // Analyze phone number exposure
            var phoneMetric = AnalyzePhoneExposure(addressesData);
            category.Metrics.Add(phoneMetric);

            // Analyze address exposure
            var addressMetric = AnalyzeAddressExposure(addressesData);
            category.Metrics.Add(addressMetric);

            // Calculate overall exposure
            var overallMetric = CalculateOverallExposure(addressesData);
            category.Metrics.Add(overallMetric);

            return Task.FromResult(category);
        }

        private PrivacyMetric AnalyzeEmailExposure(AddressData addressesData)
        {
            // Find emails in autofill entries
            var emailsInAutofill = addressesData.Autofill
                .Where(a => a.Name?.ToLower().Contains("email") == true)
                .Select(a => a.Value?.ToLower())
                .Where(v => !string.IsNullOrEmpty(v))
                .ToList();

            // Find emails in profiles
            var emailsInProfiles = addressesData.AutofillProfile
                .Where(p => !string.IsNullOrEmpty(p.Email))
                .Select(p => p.Email.ToLower())
                .ToList();

            // Find emails in contacts
            var emailsInContacts = addressesData.ContactInfo
                .Where(c => !string.IsNullOrEmpty(c.Email))
                .Select(c => c.Email.ToLower())
                .ToList();

            // Combine all emails and count total occurrences
            int totalEmailOccurrences = emailsInAutofill.Count + emailsInProfiles.Count + emailsInContacts.Count;

            // Count unique emails across all collections
            var allEmails = new HashSet<string>(emailsInAutofill);
            allEmails.UnionWith(emailsInProfiles);
            allEmails.UnionWith(emailsInContacts);
            int uniqueEmails = allEmails.Count;

            RiskLevel riskLevel;
            string recommendation;

            if (uniqueEmails == 0)
            {
                riskLevel = RiskLevel.Low;
                recommendation = "No email addresses found in autofill data. This is good for privacy.";
            }
            else if (uniqueEmails <= 2)
            {
                riskLevel = RiskLevel.Medium;
                recommendation = "Consider clearing saved email addresses periodically to reduce exposure.";
            }
            else
            {
                riskLevel = RiskLevel.High;
                recommendation = "You have multiple email addresses saved in your browser. Consider clearing this data regularly and disabling email autofill to reduce exposure risk.";
            }

            return new PrivacyMetric
            {
                Name = "Email Exposure",
                Value = $"{uniqueEmails}/{totalEmailOccurrences}", // Format: "unique/total"
                RiskLevel = riskLevel,
                Description = $"Found {uniqueEmails} unique email addresses stored across {totalEmailOccurrences} form fields in your browser.",
                Recommendation = recommendation
            };
        }

        private PrivacyMetric AnalyzePhoneExposure(AddressData addressesData)
        {
            // Find phone numbers in autofill entries
            var phonesInAutofill = addressesData.Autofill
                .Where(a => a.Name?.ToLower().Contains("phone") == true ||
                           a.Name?.ToLower().Contains("mobile") == true ||
                           a.Name?.ToLower().Contains("tel") == true)
                .Select(a => a.Value)
                .Where(v => !string.IsNullOrEmpty(v))
                .Distinct()
                .Count();

            // Find phones in profiles
            var phonesInProfiles = addressesData.AutofillProfile
                .Where(p => !string.IsNullOrEmpty(p.Phone))
                .Select(p => p.Phone)
                .Distinct()
                .Count();

            // Find phones in contacts
            var phonesInContacts = addressesData.ContactInfo
                .SelectMany(c => c.PhoneNumbers)
                .Select(p => p.Number)
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct()
                .Count();

            int totalPhones = phonesInAutofill + phonesInProfiles + phonesInContacts;

            RiskLevel riskLevel;
            string recommendation;

            if (totalPhones == 0)
            {
                riskLevel = RiskLevel.Low;
                recommendation = "No phone numbers found in autofill data. This is good for privacy.";
            }
            else if (totalPhones <= 2)
            {
                riskLevel = RiskLevel.Medium;
                recommendation = "Consider clearing saved phone numbers periodically to reduce exposure.";
            }
            else
            {
                riskLevel = RiskLevel.High;
                recommendation = "You have multiple phone numbers saved. Consider clearing this data regularly and disabling phone number autofill to reduce exposure risk.";
            }

            return new PrivacyMetric
            {
                Name = "Phone Number Exposure",
                Value = totalPhones.ToString(),
                RiskLevel = riskLevel,
                Description = $"Found {totalPhones} phone numbers stored in your browser.",
                Recommendation = recommendation
            };
        }

        private PrivacyMetric AnalyzeAddressExposure(AddressData addressesData)
        {
            // Count profiles with addresses
            var profilesWithAddresses = addressesData.AutofillProfile
                .Count(p => !string.IsNullOrEmpty(p.StreetAddress));

            // Count contacts with addresses
            var contactsWithAddresses = addressesData.ContactInfo
                .Count(c => !string.IsNullOrEmpty(c.StreetAddress));

            // Count address fields in autofill
            var addressFieldsInAutofill = addressesData.Autofill
                .Count(a => a.Name?.ToLower().Contains("address") == true ||
                           a.Name?.ToLower().Contains("street") == true ||
                           a.Name?.ToLower().Contains("city") == true ||
                           a.Name?.ToLower().Contains("zip") == true ||
                           a.Name?.ToLower().Contains("postal") == true);

            int totalAddresses = profilesWithAddresses + contactsWithAddresses +
                                (addressFieldsInAutofill > 0 ? 1 : 0);

            RiskLevel riskLevel;
            string recommendation;

            if (totalAddresses == 0)
            {
                riskLevel = RiskLevel.Low;
                recommendation = "No physical addresses found in autofill data. This is good for privacy.";
            }
            else if (totalAddresses == 1)
            {
                riskLevel = RiskLevel.Medium;
                recommendation = "Consider clearing saved address data periodically to reduce exposure.";
            }
            else
            {
                riskLevel = RiskLevel.High;
                recommendation = "You have multiple addresses saved. Consider clearing this data regularly and disabling address autofill to reduce exposure risk.";
            }

            return new PrivacyMetric
            {
                Name = "Physical Address Exposure",
                Value = totalAddresses.ToString(),
                RiskLevel = riskLevel,
                Description = $"Found {totalAddresses} physical addresses stored in your browser.",
                Recommendation = recommendation
            };
        }

        private PrivacyMetric CalculateOverallExposure(AddressData addressesData)
        {
            // Get a rough estimate of total personal data entries
            int totalEntries = addressesData.Autofill.Count +
                              addressesData.AutofillProfile.Count * 5 + // Each profile contains multiple fields
                              addressesData.ContactInfo.Count * 3;       // Each contact contains multiple fields

            RiskLevel riskLevel;
            string recommendation;

            if (totalEntries < 5)
            {
                riskLevel = RiskLevel.Low;
                recommendation = "You have minimal personal data stored in your browser. This is good for privacy.";
            }
            else if (totalEntries < 20)
            {
                riskLevel = RiskLevel.Medium;
                recommendation = "Consider reviewing and clearing unnecessary personal data from your browser regularly.";
            }
            else
            {
                riskLevel = RiskLevel.High;
                recommendation = "You have a significant amount of personal data stored in your browser. Consider clearing this data regularly and being more selective about what information you allow to be saved.";
            }

            return new PrivacyMetric
            {
                Name = "Overall Personal Data Exposure",
                Value = totalEntries.ToString(),
                RiskLevel = riskLevel,
                Description = $"Found approximately {totalEntries} personal data items stored in your browser.",
                Recommendation = recommendation
            };
        }
    }
}
