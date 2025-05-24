using System;
using System.Threading.Tasks;
using DataPrivacyAuditTool.Core.Interfaces;
using DataPrivacyAuditTool.Core.Models;

namespace DataPrivacyAuditTool.Infrastructure.Services.Analyzers
{
    /// <summary>
    /// Base class for analyzers that evaluate Settings.json file data.
    /// Provides common functionality and enforces the contract that these
    /// analyzers only require the Settings file.
    /// </summary>
    public abstract class SettingsAnalyzer:IMetricAnalyser
    {
        public AnalyzerFileType RequiredFileType => AnalyzerFileType.Settings;
        public abstract string CategoryName { get; }
        public abstract string Description { get; }

        public Task<PrivacyMetricCategory> AnalyzeAsync(ParsedGoogleData data)
        {
            if (data.SettingsData == null)
                throw new ArgumentException("Settings data is required for this analyzer");

            return AnalyzeSettingsAsync(data.SettingsData);
        }

        protected abstract Task<PrivacyMetricCategory> AnalyzeSettingsAsync(SettingsData settingsData);
    }

    /// <summary>
    /// Base class for analyzers that evaluate Addresses and more.json file data.
    /// These analyzers will be implemented after the Settings analyzers are complete.
    /// </summary>
    public abstract class AddressesAnalyzer:IMetricAnalyser
    {
        public AnalyzerFileType RequiredFileType => AnalyzerFileType.Addresses;
        public abstract string CategoryName { get; }
        public abstract string Description { get; }

        public Task<PrivacyMetricCategory> AnalyzeAsync(ParsedGoogleData data)
        {
            if (data.AddressesData == null)
                throw new ArgumentException("Addresses data is required for this analyzer");

            return AnalyzeAddressesAsync(data.AddressesData);
        }

        protected abstract Task<PrivacyMetricCategory> AnalyzeAddressesAsync(AddressData addressesData);
    }
}
