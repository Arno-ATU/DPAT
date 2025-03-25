using System.Threading.Tasks;
using DataPrivacyAuditTool.Core.Models;

namespace DataPrivacyAuditTool.Core.Interfaces
{
    public interface IMetricAnalyzer
    {
        AnalyzerFileType RequiredFileType { get; }
        string CategoryName { get; }
        string Description { get; }
        Task<PrivacyMetricCategory> AnalyzeAsync(ParsedGoogleData data);
    }

    public enum AnalyzerFileType
    {
        Settings,
        Addresses
    }
}
