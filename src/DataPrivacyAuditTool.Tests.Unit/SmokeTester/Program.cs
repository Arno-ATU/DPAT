using System;
using Microsoft.Data.Sqlite;

namespace DataPrivacyAuditTool.Tests.Unit.SmokeTester
{
    class Program
    {
        static int Main(string[] args)
        {
            string environment = "Development";

            // Parse command line args
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--environment" && i + 1 < args.Length)
                {
                    environment = args[i + 1];
                }
            }

            Console.WriteLine($"Running smoke test for {environment} environment");

            // Determine which database file to use
            string dbPath = environment.ToLower() switch
            {
                "development" => "dpat_development.db",
                "staging" => "dpat_staging.db",
                "production" => "dpat_production.db",
                _ => "dpat_test.db"
            };

            try
            {
                // Test database connectivity
                using var connection = new SqliteConnection($"Data Source=src/DataPrivacyAuditTool/{dbPath}");
                connection.Open();

                // Verify AuditHistories table exists
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='AuditHistories'";
                var result = Convert.ToInt32(cmd.ExecuteScalar());

                if (result == 0)
                {
                    Console.WriteLine("ERROR: AuditHistories table does not exist");
                    return 1;
                }

                // Test basic read operations
                cmd.CommandText = "SELECT COUNT(*) FROM AuditHistories";
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                Console.WriteLine($"Found {count} audit records in database");

                // Test basic write operations (test transaction, don't commit)
                using var transaction = connection.BeginTransaction();
                cmd.Transaction = transaction;
                cmd.CommandText = @"
                    INSERT INTO AuditHistories (Username, OverallScore, AuditDate, CreatedAt) 
                    VALUES ('SmokeTest', 99.9, datetime('now'), datetime('now'))";
                cmd.ExecuteNonQuery();

                // Validate the insert worked
                cmd.CommandText = "SELECT COUNT(*) FROM AuditHistories WHERE Username = 'SmokeTest'";
                var smokeTestCount = Convert.ToInt32(cmd.ExecuteScalar());

                if (smokeTestCount != 1)
                {
                    Console.WriteLine("ERROR: Failed to insert test data");
                    return 1;
                }

                // Rollback the transaction to leave database clean
                transaction.Rollback();

                Console.WriteLine("Smoke test completed successfully!");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Smoke test failed: {ex.Message}");
                return 1;
            }
        }
    }
}
