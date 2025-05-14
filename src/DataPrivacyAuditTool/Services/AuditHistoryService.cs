using DataPrivacyAuditTool.Core.Interfaces;
using DataPrivacyAuditTool.Core.Models;
using DataPrivacyAuditTool.Data;
using Microsoft.EntityFrameworkCore;

namespace DataPrivacyAuditTool.Services
{
    public class AuditHistoryService : IAuditHistoryService
    {
        private readonly DpatDbContext _context;

        public AuditHistoryService(DpatDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveAuditAsync(string username, PrivacyAnalysisResult result)
        {
            if (string.IsNullOrWhiteSpace(username))
                return 0;

            var auditHistory = new AuditHistory
            {
                Username = username.Trim(),
                AuditDate = result.AnalysisDate,
                OverallScore = result.OverallScore,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditHistories.Add(auditHistory);
            await _context.SaveChangesAsync();

            return auditHistory.Id;
        }

        public async Task<List<AuditHistory>> GetUserAuditHistoryAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return new List<AuditHistory>();

            return await _context.AuditHistories
                .Where(ah => ah.Username == username.Trim())
                .OrderByDescending(ah => ah.AuditDate)
                .ToListAsync();
        }

        public async Task<AuditHistory?> GetLatestAuditAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            return await _context.AuditHistories
                .Where(ah => ah.Username == username.Trim())
                .OrderByDescending(ah => ah.AuditDate)
                .FirstOrDefaultAsync();
        }
    }
}
