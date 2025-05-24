using System.Threading.Tasks;
using DataPrivacyAuditTool.Core.Models;

namespace DataPrivacyAuditTool.Core.Interfaces
{
    public interface IAnalyserEngine
    {
        Task<PrivacyAnalysisResult> ExecuteAnalysisAsync(ParsedGoogleData data);
    }
}
