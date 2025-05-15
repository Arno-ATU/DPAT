using DataPrivacyAuditTool.Core.Models;

namespace DataPrivacyAuditTool.Core.Interfaces
{
    public interface IAuditHistoryService
    {
        Task<int> SaveAuditAsync(string username, PrivacyAnalysisResult result);
        Task<List<AuditHistory>> GetUserAuditHistoryAsync(string username);
        Task<AuditHistory?> GetLatestAuditAsync(string username);
        Task<List<AuditHistory>> GetAllAuditsAsync();
    }
}
