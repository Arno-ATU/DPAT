using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataPrivacyAuditTool.Core.Models;

namespace DataPrivacyAuditTool.Infrastructure.Services.Analyzers
{
    public class ExtensionPrivacyAnalyzer : SettingsAnalyzer
    {
        public override string CategoryName => "Browser Extensions";
        public override string Description => "Analyzes browser extensions for potential privacy implications";

        // Known privacy-enhancing extensions (common privacy extensions by ID)
        private static readonly HashSet<string> PrivacyExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "cjpalhdlnbpafiamejdnhcphjbkeiagm", // uBlock Origin
            "gcbommkclmclpchllfjekcdonpmejbdp", // HTTPS Everywhere
            "pkehgijcmpdhfbdbbnkijodmdjhbjlgp", // Privacy Badger
            "gppongmhjkpfnbhagpmjfkannfbllamg", // Decentraleyes
            "ldpochfccmkkmhdbclfhpagapcfdljkj", // Disconnect
            "kbfnbcaeplbcioakkpcpgfkobkghlhen", // Ghostery
            "adbcdjfejgmnjnjdebcpoepcfibacnoe", // ClearURLs
            "doojmbjmlfjjnbmnoijecmcbfeoakpjm", // NoScript
            "fihnjjcciajhdojfnbdddfaoknhalnja"  // I don't care about cookies
        };

        protected override Task<PrivacyMetricCategory> AnalyzeSettingsAsync(SettingsData settingsData)
        {
            var category = new PrivacyMetricCategory
            {
                Name = CategoryName,
                Description = Description,
                Metrics = new List<PrivacyMetric>()
            };

            // Add the extension count/risk metric
            var extensionMetric = AnalyzeExtensions(settingsData);
            category.Metrics.Add(extensionMetric);

            // Add the incognito enabled extensions metric
            var incognitoMetric = AnalyzeIncognitoExtensions(settingsData);
            category.Metrics.Add(incognitoMetric);

            return Task.FromResult(category);
        }

        private PrivacyMetric AnalyzeExtensions(SettingsData settingsData)
        {
            // Count total extensions
            int totalExtensions = settingsData.Apps?.Count ?? 0;

            // Count privacy extensions
            int privacyExtensionsCount = 0;

            if (settingsData.Apps != null)
            {
                foreach (var app in settingsData.Apps)
                {
                    if (app.Extension != null && PrivacyExtensions.Contains(app.Extension.Id))
                    {
                        privacyExtensionsCount++;
                    }
                }
            }

            // Determine risk level based on extension count and privacy extensions ratio
            RiskLevel riskLevel;
            string recommendation;

            if (totalExtensions == 0)
            {
                riskLevel = RiskLevel.Low;
                recommendation = "You have no browser extensions installed. This reduces the risk of privacy leaks through third-party code.";
            }
            else if (totalExtensions <= 3)
            {
                riskLevel = RiskLevel.Low;
                recommendation = "You have few browser extensions installed. This is good for privacy, as each extension can potentially access your browsing data.";
            }
            else if (totalExtensions <= 7)
            {
                if (privacyExtensionsCount > 0)
                {
                    riskLevel = RiskLevel.Medium;
                    recommendation = "You have several browser extensions installed, including privacy-enhancing ones. Consider reviewing your non-privacy extensions to ensure they're trustworthy.";
                }
                else
                {
                    riskLevel = RiskLevel.Medium;
                    recommendation = "You have several browser extensions installed. Consider using privacy-focused extensions and removing any that aren't essential.";
                }
            }
            else
            {
                riskLevel = RiskLevel.High;
                recommendation = "You have many browser extensions installed. Each extension can potentially access your browsing data. Consider removing non-essential extensions and prioritizing privacy-focused ones.";
            }

            string description = $"Found {totalExtensions} browser extensions installed";
            if (privacyExtensionsCount > 0)
            {
                description += $", including {privacyExtensionsCount} privacy-enhancing extensions";
            }
            description += ".";

            return new PrivacyMetric
            {
                Name = "Extension Count",
                Value = totalExtensions.ToString(),
                RiskLevel = riskLevel,
                Description = description,
                Recommendation = recommendation
            };
        }

        private PrivacyMetric AnalyzeIncognitoExtensions(SettingsData settingsData)
        {
            int incognitoEnabledCount = 0;

            if (settingsData.Apps != null)
            {
                foreach (var app in settingsData.Apps)
                {
                    if (app.Extension != null && app.Extension.IncognitoEnabled)
                    {
                        incognitoEnabledCount++;
                    }
                }
            }

            RiskLevel riskLevel;
            string recommendation;

            if (incognitoEnabledCount == 0)
            {
                riskLevel = RiskLevel.Low;
                recommendation = "No extensions are enabled in incognito mode. This helps maintain privacy during private browsing sessions.";
            }
            else if (incognitoEnabledCount <= 2)
            {
                riskLevel = RiskLevel.Medium;
                recommendation = "A few extensions are enabled in incognito mode. Consider disabling non-essential extensions in incognito for enhanced privacy.";
            }
            else
            {
                riskLevel = RiskLevel.High;
                recommendation = "Many extensions are enabled in incognito mode. Extensions can track your activity even in private browsing. Consider disabling most extensions in incognito.";
            }

            string description = $"Found {incognitoEnabledCount} extensions enabled in incognito mode. Extensions can still access your browsing data in private browsing sessions.";

            return new PrivacyMetric
            {
                Name = "Incognito Extensions",
                Value = incognitoEnabledCount.ToString(),
                RiskLevel = riskLevel,
                Description = description,
                Recommendation = recommendation
            };
        }
    }
}
