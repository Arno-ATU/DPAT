using DataPrivacyAuditTool.Core.Models;
using DataPrivacyAuditTool.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace DataPrivacyAuditTool.Tests.Unit.Services
{
    public class DatabaseValidationTests
    {
        [Fact]
        public void Database_CanStoreAndRetrieveAuditHistory()
        {
            // Arrange - Create in-memory test database
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<DpatDbContext>()
                .UseSqlite(connection)
                .Options;

            // Create the schema
            using (var context = new DpatDbContext(options))
            {
                context.Database.EnsureCreated();
            }

            // Setup mock data
            var testUser = "TestUser123";
            var testScore = 85.5;
            var testDate = DateTime.UtcNow.AddDays(-1);

            // Act - Save data to database
            using (var context = new DpatDbContext(options))
            {
                var auditHistory = new AuditHistory
                {
                    Username = testUser,
                    OverallScore = testScore,
                    AuditDate = testDate,
                    CreatedAt = DateTime.UtcNow
                };

                context.AuditHistories.Add(auditHistory);
                context.SaveChanges();
            }

            // Assert - Retrieve and verify data
            using (var context = new DpatDbContext(options))
            {
                var savedAudit = context.AuditHistories.FirstOrDefault(a => a.Username == testUser);

                Assert.NotNull(savedAudit);
                Assert.Equal(testUser, savedAudit.Username);
                Assert.Equal(testScore, savedAudit.OverallScore);

                // Date comparison - check for same day
                Assert.Equal(testDate.Date, savedAudit.AuditDate.Date);

                // Verify CreatedAt was populated
                Assert.True(savedAudit.CreatedAt > DateTime.UtcNow.AddMinutes(-5));
            }
        }
    }
}
