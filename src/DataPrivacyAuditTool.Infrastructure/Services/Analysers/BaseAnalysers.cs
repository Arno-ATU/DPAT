using System;
using System.Threading.Tasks;
using DataPrivacyAuditTool.Core.Interfaces;
using DataPrivacyAuditTool.Core.Models;

namespace DataPrivacyAuditTool.Infrastructure.Services.Analysers
{
    /// <summary>
    /// Base class for analyzers that evaluate Settings.json file data.
    /// Provides common functionality and enforces the contract that these
    /// analyzers only require the Settings file.
    /// </summary>
    public abstract class SettingsAnalyser : IMetricAnalyser
    {
        public AnalyserFileType RequiredFileType => AnalyserFileType.Settings;
        public abstract string CategoryName { get; }
        public abstract string Description { get; }

        public Task<PrivacyMetricCategory> AnalyseAsync(ParsedGoogleData data)
        {
            if (data.SettingsData == null)
                throw new ArgumentException("Settings data is required for this analyser");

            return AnalyseSettingsAsync(data.SettingsData);
        }

        protected abstract Task<PrivacyMetricCategory> AnalyseSettingsAsync(SettingsData settingsData);
    }

    /// <summary>
    /// Base class for analyzers that evaluate Addresses and more.json file data.
    /// These analyzers will be implemented after the Settings analyzers are complete.
    /// </summary>
    public abstract class AddressesAnalyser:IMetricAnalyser
    {
        public AnalyserFileType RequiredFileType => AnalyserFileType.Addresses;
        public abstract string CategoryName { get; }
        public abstract string Description { get; }

        public Task<PrivacyMetricCategory> AnalyseAsync(ParsedGoogleData data)
        {
            if (data.AddressesData == null)
                throw new ArgumentException("Addresses data is required for this analyser");

            return AnalyseAddressesAsync(data.AddressesData);
        }

        protected abstract Task<PrivacyMetricCategory> AnalyseAddressesAsync(AddressData addressesData);
    }
}
