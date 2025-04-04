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

            // Count total occurrences/forms where emails are stored
            int totalEmailForms = emailsInAutofill.Count + emailsInProfiles.Count + emailsInContacts.Count;

            // Normalize and find unique emails across all collections
            var uniqueEmails = new HashSet<string>();
            foreach (var email in emailsInAutofill.Concat(emailsInProfiles).Concat(emailsInContacts))
            {
                uniqueEmails.Add(NormalizeEmail(email));
            }

            // Remove empty strings that might have been added during normalization
            uniqueEmails.Remove("");
            int uniqueEmailCount = uniqueEmails.Count;

            // Determine risk level based on number of forms
            RiskLevel riskLevel;
            string recommendation;

            if (totalEmailForms <= 4)
            {
                riskLevel = RiskLevel.Low;
                recommendation = "You have relatively few forms storing email addresses. This is good for privacy.";
            }
            else if (totalEmailForms <= 8)
            {
                riskLevel = RiskLevel.Medium;
                recommendation = "Consider clearing saved email addresses periodically to reduce exposure risk.";
            }
            else if (totalEmailForms <= 10)
            {
                riskLevel = RiskLevel.High;
                recommendation = "Your email addresses are stored in many form fields. Consider clearing this data regularly and disabling email autofill to reduce exposure risk.";
            }
            else
            {
                riskLevel = RiskLevel.Critical;
                recommendation = "Your email addresses are stored in a large number of form fields. This creates significant data exposure risk. Clear this data regularly and be selective about which sites can store your information.";
            }

            return new PrivacyMetric
            {
                Name = "Email Exposure",
                Value = $"{uniqueEmailCount}/{totalEmailForms}", // Format: "unique/total forms"
                RiskLevel = riskLevel,
                Description = $"Found {uniqueEmailCount} unique email addresses stored across {totalEmailForms} form fields in your browser.",
                Recommendation = recommendation
            };
        }

        // Helper method to normalize email addresses
        private string NormalizeEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return string.Empty;

            // Lowercase and trim
            string normalized = email.ToLowerInvariant().Trim();

            // Optional: Filter out obvious test data
            if (normalized.Contains("@email.com") || normalized.Contains("@example.com"))
                return string.Empty;

            return normalized;
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
                .ToList();

            // Find phones in profiles
            var phonesInProfiles = addressesData.AutofillProfile
                .Where(p => !string.IsNullOrEmpty(p.Phone))
                .Select(p => p.Phone)
                .ToList();

            // Find phones in contacts
            var phonesInContacts = addressesData.ContactInfo
                .SelectMany(c => c.PhoneNumbers)
                .Select(p => p.Number)
                .Where(n => !string.IsNullOrEmpty(n))
                .ToList();

            // Count total occurrences/forms where phone numbers are stored
            int totalPhoneForms = phonesInAutofill.Count + phonesInProfiles.Count + phonesInContacts.Count;

            // Normalize and find unique phone numbers across all collections
            var uniquePhones = new HashSet<string>();
            foreach (var phone in phonesInAutofill.Concat(phonesInProfiles).Concat(phonesInContacts))
            {
                uniquePhones.Add(NormalizePhoneNumber(phone));
            }

            // Remove empty strings that might have been added during normalization
            uniquePhones.Remove("");
            int uniquePhoneCount = uniquePhones.Count;

            // Determine risk level based on number of forms
            RiskLevel riskLevel;
            string recommendation;

            if (totalPhoneForms <= 4)
            {
                riskLevel = RiskLevel.Low;
                recommendation = "You have relatively few forms storing phone numbers. This is good for privacy.";
            }
            else if (totalPhoneForms <= 8)
            {
                riskLevel = RiskLevel.Medium;
                recommendation = "Consider clearing saved phone numbers periodically to reduce exposure risk.";
            }
            else if (totalPhoneForms <= 10)
            {
                riskLevel = RiskLevel.High;
                recommendation = "Your phone numbers are stored in many form fields. Consider clearing this data regularly and disabling phone number autofill to reduce exposure risk.";
            }
            else
            {
                riskLevel = RiskLevel.Critical;
                recommendation = "Your phone numbers are stored in a large number of form fields. This creates significant data exposure risk. Clear this data regularly and be selective about which sites can store your information.";
            }

            return new PrivacyMetric
            {
                Name = "Phone Number Exposure",
                Value = $"{uniquePhoneCount}/{totalPhoneForms}", // Format: "unique/total forms"
                RiskLevel = riskLevel,
                Description = $"Found {uniquePhoneCount} unique phone numbers stored across {totalPhoneForms} form fields in your browser.",
                Recommendation = recommendation
            };
        }

        // Helper method to normalize phone numbers
        private string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return string.Empty;

            // Remove all non-digit characters
            string digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());

            // Simple handling for common country code formats
            // If it starts with 353 (Ireland), normalize to 0
            if (digitsOnly.StartsWith("353") && digitsOnly.Length > 9)
                return "0" + digitsOnly.Substring(3);

            return digitsOnly;
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
