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
            string dbFileName = environment.ToLower() switch
            {
                "development" => "dpat_development.db",
                "staging" => "dpat_staging.db",
                "production" => "dpat_production.db",
                _ => "dpat_test.db"
            };

            // Build path that works in both local and CI environments
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = FindProjectRoot(baseDirectory);
            string dbPath = Path.Combine(projectRoot, "src", "DataPrivacyAuditTool", dbFileName);

            Console.WriteLine($"Running smoke test for {environment} environment");
            Console.WriteLine($"Looking for database at: {dbPath}");

            try
            {
                // Connect to the database
                using var connection = new SqliteConnection($"Data Source={dbPath}");
                connection.Open();

                // Rest of your test code...
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Smoke test failed: {ex.Message}");
                Assert.True(false, $"Smoke test failed: {ex.Message}. Database path: {dbPath}");
            }
        }

        private string FindProjectRoot(string startDirectory)
        {
            // Walk up the directory tree until we find the repository root
            var currentDirectory = new DirectoryInfo(startDirectory);

            while (currentDirectory != null)
            {
                // Check if this is the repository root
                if (Directory.Exists(Path.Combine(currentDirectory.FullName, ".git")) ||
                    File.Exists(Path.Combine(currentDirectory.FullName, "DataPrivacyAuditTool.sln")))
                {
                    return currentDirectory.FullName;
                }

                // Move up one directory
                currentDirectory = currentDirectory.Parent;
            }

            // Return the base directory if cant find the root
            return startDirectory;
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
