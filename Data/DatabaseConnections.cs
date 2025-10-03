// Data/DatabaseConnection.cs
using Npgsql; // Changed from MySql.Data.MySqlClient
using System;

namespace Electronics_Shop2.Data
{
    public class DatabaseConnection
    {
        private readonly string _connectionString;

        public DatabaseConnection()
        {
            _connectionString = "x";
        }

        public NpgsqlConnection GetConnection() // Changed return type
        {
            return new NpgsqlConnection(_connectionString);
        }

        public bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    Console.WriteLine("✅ Database connection successful!");
                    return true;
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