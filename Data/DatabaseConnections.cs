// Data/DatabaseConnection.cs
using Npgsql;
using System;

namespace Electronics_Shop2.Data
{
    public class DatabaseConnection
    {
        private readonly string _connectionString;

        public DatabaseConnection()
        {
            // Simple connection string with built-in pooling
            _connectionString = "Host=ep-rapid-block-ag0qblkb-pooler.c-2.eu-central-1.aws.neon.tech;" +
                               "Database=electronics_shopdbTEST;" +
                               "Username=neondb_owner;" +
                               "Password=;" +
                               "SSL Mode=Require;" +
                               "Trust Server Certificate=true;" +
                               "Pooling=true;" +
                               "Timeout=15;" + // Connection timeout
                               "Command Timeout=30"; // Query timeout
        }

        public NpgsqlConnection GetConnection()
        {
            var connection = new NpgsqlConnection(_connectionString);

            try
            {
                connection.Open();
                return connection;
            }
            catch (NpgsqlException ex) when (ex.IsTransient)
            {
                // Retry on transient errors
                System.Threading.Thread.Sleep(1000); // Wait 1 second
                connection = new NpgsqlConnection(_connectionString);
                connection.Open();
                return connection;
            }
        }

        public bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand("SELECT 1", connection))
                    {
                        command.ExecuteScalar();
                        Console.WriteLine("✅ Database connection successful!");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database connection failed: {ex.Message}");
                return false;
            }
        }
    }
}