using Npgsql;
using Npgsql;

namespace Electronics_Shop2.Data
{
    public abstract class BaseManager
    {
        protected NpgsqlConnection connection;
        protected DatabaseConnection dbConnection;

        public BaseManager()
        {
            dbConnection = new DatabaseConnection();
            connection = dbConnection.GetConnection();
        }
        public void UseExistingConnection(NpgsqlConnection existingConnection)
        {
            // Close current connection if open
            if (connection?.State == System.Data.ConnectionState.Open)
                connection.Close();

            // Use the provided connection
            connection = existingConnection;
        }
        protected void EnsureConnectionOpen()
        {
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();
        }

        protected void EnsureConnectionClosed()
        {
            if (connection.State == System.Data.ConnectionState.Open)
                connection.Close();
        }

        protected string GetSafeString(object? value)
        {
            return value == null || value == DBNull.Value ? string.Empty : value.ToString() ?? string.Empty;
        }

        protected int GetSafeInt(object? value)
        {
            if (value == null || value == DBNull.Value) return 0;
            return Convert.ToInt32(value);
        }

        protected decimal GetSafeDecimal(object? value)
        {
            if (value == null || value == DBNull.Value) return 0;
            return Convert.ToDecimal(value);
        }

        protected void HandleException(Exception ex, string operation)
        {
            Console.WriteLine($"❌ Error during {operation}: {ex.Message}");
        }
    }
}