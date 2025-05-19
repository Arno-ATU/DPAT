using System;
using DataPrivacyAuditTool.Data;
using Microsoft.Data.Sqlite;
using Xunit;

namespace DataPrivacyAuditTool.Tests.Unit.Services
{
    public class DatabaseSmokeTests
    {
        [Fact]
        public void Development_Database_BasicOperationsWork()
        {
            RunSmokeTest("Development");
        }

        [Fact]
        public void Staging_Database_BasicOperationsWork()
        {
            RunSmokeTest("Staging");
        }

        [Fact]
        public void Production_Database_BasicOperationsWork()
        {
            RunSmokeTest("Production");
        }

        private void RunSmokeTest(string environment)
        {
            // Skip test if we're not specifically targeting this environment
            if (!ShouldRunForEnvironment(environment))
            {
                return;
            }

            // Determine which database file to use
            string dbPath = environment.ToLower() switch
            {
                "development" => "dpat_development.db",
                "staging" => "dpat_staging.db",
                "production" => "dpat_production.db",
                _ => "dpat_test.db"
            };

            Console.WriteLine($"Running smoke test for {environment} environment");

            try
            {
                // Connect to the database
                using var connection = new SqliteConnection($"Data Source=src/DataPrivacyAuditTool/{dbPath}");
                connection.Open();

                // Verify AuditHistories table exists
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='AuditHistories'";
                var result = Convert.ToInt32(cmd.ExecuteScalar());

                Assert.True(result > 0, "AuditHistories table does not exist");

                // Test basic read operations
                cmd.CommandText = "SELECT COUNT(*) FROM AuditHistories";
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                Console.WriteLine($"Found {count} audit records in database");

                // Test basic write operations (using a transaction that will be rolled back)
                using var transaction = connection.BeginTransaction();
                cmd.Transaction = transaction;
                cmd.CommandText = @"
                    INSERT INTO AuditHistories (Username, OverallScore, AuditDate, CreatedAt) 
                    VALUES ('SmokeTest', 99.9, datetime('now'), datetime('now'))";
                cmd.ExecuteNonQuery();

                // Validate the insert worked
                cmd.CommandText = "SELECT COUNT(*) FROM AuditHistories WHERE Username = 'SmokeTest'";
                var smokeTestCount = Convert.ToInt32(cmd.ExecuteScalar());

                Assert.Equal(1, smokeTestCount);

                // Rollback the transaction to leave database clean
                transaction.Rollback();

                Console.WriteLine("Smoke test completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Smoke test failed: {ex.Message}");
                Assert.True(false, $"Smoke test failed: {ex.Message}");
            }
        }

        private bool ShouldRunForEnvironment(string environment)
        {
            // This method will determine whether to run based on environment variables
            // For local development, we can use environment variables
            var targetEnv = Environment.GetEnvironmentVariable("DPAT_TEST_ENVIRONMENT");

            // If no specific environment is targeted, run for all environments
            if (string.IsNullOrEmpty(targetEnv))
            {
                return true;
            }

            // Run only if the targeted environment matches the test
            return string.Equals(targetEnv, environment, StringComparison.OrdinalIgnoreCase);
        }
    }
}
