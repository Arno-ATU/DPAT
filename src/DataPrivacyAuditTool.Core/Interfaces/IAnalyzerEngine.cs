using System.Threading.Tasks;
using DataPrivacyAuditTool.Core.Models;

namespace DataPrivacyAuditTool.Core.Interfaces
{
    public interface IAnalyzerEngine
    {
        Task<PrivacyAnalysisResult> ExecuteAnalysisAsync(ParsedGoogleData data);
    }
}
