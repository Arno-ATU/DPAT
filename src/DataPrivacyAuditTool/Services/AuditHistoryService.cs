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
            Console.WriteLine($"SaveAuditAsync called with username: '{username}'");

            if (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine("Username is null or empty, returning 0");
                return 0;
            }

            // Trim the milliseconds from timestamps for cleaner storage
            var auditDate = new DateTime(result.AnalysisDate.Year, result.AnalysisDate.Month, result.AnalysisDate.Day,
                                        result.AnalysisDate.Hour, result.AnalysisDate.Minute, result.AnalysisDate.Second);

            var createdAt = DateTime.Now;
            createdAt = new DateTime(createdAt.Year, createdAt.Month, createdAt.Day,
                                   createdAt.Hour, createdAt.Minute, createdAt.Second);

            var auditHistory = new AuditHistory
            {
                Username = username.Trim(),
                AuditDate = auditDate,  // Milliseconds trimmed
                OverallScore = result.OverallScore,
                CreatedAt = createdAt  // Milliseconds trimmed
            };

            Console.WriteLine($"Created audit history object: {auditHistory.Username}, Score: {auditHistory.OverallScore}");

            _context.AuditHistories.Add(auditHistory);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Saved to database with ID: {auditHistory.Id}");
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
