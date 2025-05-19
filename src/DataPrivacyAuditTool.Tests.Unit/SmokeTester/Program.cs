using System;
using Microsoft.Data.Sqlite;


namespace DataPrivacyAuditTool.Tests.Unit.SmokeTester
{

    public class DatabaseSmokeTest
    {
        // Use the explicit [STAThread] attribute to clarify this is the entry point
        [STAThread]
        public static int Main(string[] args)
        {
            // Default to Development if no environment is specified
            string envName = "Development";

            // Parse command line args
            for (int i = 0; i < args.Length; i++)
            {
                // Look for the --environment flag and take the value that follows it
                if (args[i] == "--environment" && i + 1 < args.Length)
                {
                    envName = args[i + 1];
                }
            }

            Console.WriteLine($"Running smoke test for {envName} environment");

            // Determine which database file to use based on the environment
            string dbPath = envName.ToLower() switch
            {
                "development" => "dpat_development.db",
                "staging" => "dpat_staging.db",
                "production" => "dpat_production.db",
                _ => "dpat_test.db"
            };

            try
            {
                // Connect to the database
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
