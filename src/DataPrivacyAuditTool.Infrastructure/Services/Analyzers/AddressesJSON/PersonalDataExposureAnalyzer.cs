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
                recommendation = "Consider reviewing and clearing saved phone numbers from form fields periodically to reduce exposure risk.";
            }
            else if (totalPhoneForms <= 10)
            {
                riskLevel = RiskLevel.High;
                recommendation = "Your phone numbers are stored up to 10 different form fields. " +
                                 "Consider clearing this data regularly and disabling phone number autofill to reduce exposure risk.";
            }
            else
            {
                riskLevel = RiskLevel.Critical;
                recommendation = "Your phone numbers are stored in more than 10 different form fields. " +
                                 "This creates significant data exposure risk. Clear this data regularly and be selective about which sites can store your information.";
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
            // Track forms with address data
            var addressFormsList = new List<string>();

            // Count profiles with addresses
            foreach (var profile in addressesData.AutofillProfile)
            {
                if (!string.IsNullOrEmpty(profile.StreetAddress))
                {
                    addressFormsList.Add($"Profile: {profile.Guid}");
                }
            }

            // Count contacts with addresses
            foreach (var contact in addressesData.ContactInfo)
            {
                if (!string.IsNullOrEmpty(contact.StreetAddress))
                {
                    addressFormsList.Add($"Contact: {contact.Guid}");
                }
            }

            // Count address fields in autofill
            foreach (var entry in addressesData.Autofill)
            {
                if (entry.Name?.ToLower().Contains("address") == true ||
                    entry.Name?.ToLower().Contains("street") == true ||
                    entry.Name?.ToLower().Contains("city") == true ||
                    entry.Name?.ToLower().Contains("zip") == true ||
                    entry.Name?.ToLower().Contains("postal") == true)
                {
                    addressFormsList.Add($"Autofill: {entry.Name}");
                }
            }

            // Get unique addresses (normalizing as best as possible for now)
            var uniqueAddresses = new HashSet<string>();

            // Add profile addresses to unique set
            foreach (var profile in addressesData.AutofillProfile)
            {
                if (!string.IsNullOrEmpty(profile.StreetAddress))
                {
                    uniqueAddresses.Add(NormalizeAddress(profile.StreetAddress));
                }
            }

            // Add contact addresses to unique set
            foreach (var contact in addressesData.ContactInfo)
            {
                if (!string.IsNullOrEmpty(contact.StreetAddress))
                {
                    uniqueAddresses.Add(NormalizeAddress(contact.StreetAddress));
                }
            }

            // Add autofill addresses to unique set
            foreach (var entry in addressesData.Autofill.Where(a =>
                a.Name?.ToLower().Contains("address") == true &&
                !string.IsNullOrEmpty(a.Value)))
            {
                uniqueAddresses.Add(NormalizeAddress(entry.Value));
            }

            // Remove empty strings
            uniqueAddresses.Remove("");

            int totalAddressForms = addressFormsList.Count;
            int uniqueAddressCount = uniqueAddresses.Count;

            // Determine risk level based on number of forms
            RiskLevel riskLevel;
            string description;
            string recommendation;

            if (uniqueAddressCount == 0)
            {
                riskLevel = RiskLevel.Low;
                description = "No physical addresses found in your browser data.";
                recommendation = "This is good for privacy.";
            }
            else if (totalAddressForms <= 4)
            {
                riskLevel = RiskLevel.Low;
                description = $"Found {uniqueAddressCount} unique physical address(es) stored across {totalAddressForms} form fields in your browser.";
                recommendation = "You have relatively few forms storing address information. Consider clearing this data periodically to maintain privacy.";
            }
            else if (totalAddressForms <= 8)
            {
                riskLevel = RiskLevel.Medium;
                description = $"Found {uniqueAddressCount} unique physical address(es) stored across {totalAddressForms} form fields in your browser.";
                recommendation = "Consider clearing saved address data periodically to reduce exposure risk.";
            }
            else if (totalAddressForms <= 10)
            {
                riskLevel = RiskLevel.High;
                description = $"Found {uniqueAddressCount} unique physical address(es) stored across {totalAddressForms} form fields in your browser.";
                recommendation = "Your address information is stored in many form fields. Consider clearing this data regularly and limiting address autofill.";
            }
            else
            {
                riskLevel = RiskLevel.Critical;
                description = $"Found {uniqueAddressCount} unique physical address(es) stored across {totalAddressForms} form fields in your browser.";
                recommendation = "Your address information is stored in a large number of form fields. This creates significant privacy risk. Clear this data regularly.";
            }

            return new PrivacyMetric
            {
                Name = "Physical Address Exposure",
                Value = $"{uniqueAddressCount}/{totalAddressForms}",
                RiskLevel = riskLevel,
                Description = description,
                Recommendation = recommendation
            };
        }

        // Helper method to normalize addresses
        private string NormalizeAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
                return string.Empty;

            // Basic normalization - lowercase and trim
            string normalized = address.ToLowerInvariant().Trim();


            return normalized;
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
            else if (totalEntries < 25)
            {
                riskLevel = RiskLevel.Medium;
                recommendation = "You have under 25 individual items of data stored across various form fields in your browser. " +
                                 "Consider reviewing and clearing unnecessary personal data from your browser regularly.";
            }
            else
            {
                riskLevel = RiskLevel.High;
                recommendation = "You have a significant amount of personal data stored in your browser. " +
                                 "Consider clearing this data regularly and being more selective about what information you allow to be saved.";
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
