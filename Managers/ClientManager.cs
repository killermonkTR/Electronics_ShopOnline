using Electronics_Shop2.Data;
using Npgsql;

namespace Electronics_Shop2.Managers
{
    public class ClientManager : BaseManager
    {
       
        public void ShowClientManagement()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("👥 CLIENT MANAGEMENT");
                Console.WriteLine("1. View All Clients");
                Console.WriteLine("2. Add New Client");
                Console.WriteLine("3. Update Client");
                Console.WriteLine("4. Remove Client");
                Console.WriteLine("5. Back to Main Menu");
                Console.Write("Choose option: ");

                try
                {
                    EnsureConnectionOpen();

                    switch (Console.ReadLine())
                    {
                        case "1": ViewAllClients(); break;
                        case "2": AddNewClient(); break;
                        case "3": UpdateClient(); break;
                        case "4": RemoveClient(); break;
                        case "5": return;
                        default: Console.WriteLine("Invalid option!"); break;
                    }
                }
                catch (Exception ex)
                {
                    HandleException(ex, "client management");
                }
                finally
                {
                    EnsureConnectionClosed();
                }

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }

        public void ViewAllClients()
        {
            Console.WriteLine("\n=== All Clients ===");
            string query = "SELECT * FROM Clients ORDER BY Name";

            using var command = new NpgsqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader["id_Client"]} | Name: {GetSafeString(reader["Name"])}");
                Console.WriteLine($"   Email: {GetSafeString(reader["Email"])} | Phone: {GetSafeString(reader["Phone_Number"])}");
                Console.WriteLine($"   Address: {GetSafeString(reader["Address"])}");
                Console.WriteLine("----------------------------------------");
            }
        }

        public int AddNewClient()
        {
            Console.Write("Enter client name: ");
            string name = Console.ReadLine() ?? "";
            Console.Write("Enter email: ");
            string email = Console.ReadLine() ?? "";
            Console.Write("Enter phone number: ");
            string phone = Console.ReadLine() ?? "";
            Console.Write("Enter address: ");
            string address = Console.ReadLine() ?? "";

            string query = @"INSERT INTO Clients (Name, Email, Phone_Number, Address, Registration_Date) 
                            VALUES (@name, @email, @phone, @address, CURDATE()); 
                            SELECT LAST_INSERT_ID();";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@email", string.IsNullOrEmpty(email) ? DBNull.Value : email);
            command.Parameters.AddWithValue("@phone", string.IsNullOrEmpty(phone) ? DBNull.Value : phone);
            command.Parameters.AddWithValue("@address", string.IsNullOrEmpty(address) ? DBNull.Value : address);

            int newId = Convert.ToInt32(command.ExecuteScalar());
            Console.WriteLine($"✅ Client '{name}' added with ID: {newId}");
            return newId;
        }

    


public void UpdateClient()
        {
            ViewAllClients();
            Console.Write("\nEnter client ID to update: ");
            if (!int.TryParse(Console.ReadLine(), out int clientId)) return;

            Console.Write("Enter new name: ");
            string name = Console.ReadLine();
            Console.Write("Enter new email: ");
            string email = Console.ReadLine();
            Console.Write("Enter new phone: ");
            string phone = Console.ReadLine();
            Console.Write("Enter new address: ");
            string address = Console.ReadLine();

            string query = @"UPDATE Clients SET Name = @name, Email = @email, 
                        Phone_Number = @phone, Address = @address 
                        WHERE id_Client = @id";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@email", string.IsNullOrEmpty(email) ? DBNull.Value : email);
            command.Parameters.AddWithValue("@phone", string.IsNullOrEmpty(phone) ? DBNull.Value : phone);
            command.Parameters.AddWithValue("@address", string.IsNullOrEmpty(address) ? DBNull.Value : address);
            command.Parameters.AddWithValue("@id", clientId);

            int affected = command.ExecuteNonQuery();
            Console.WriteLine(affected > 0 ? "✅ Client updated!" : "❌ Client not found!");
        }

        public void RemoveClient()
        {
            ViewAllClients();
            Console.Write("\nEnter client ID to remove: ");
            if (!int.TryParse(Console.ReadLine(), out int clientId)) return;

            // Check for existing orders
            string checkQuery = "SELECT COUNT(*) FROM Orders WHERE id_Client = @id";
            using var checkCmd = new NpgsqlCommand(checkQuery, connection);
            checkCmd.Parameters.AddWithValue("@id", clientId);

            if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
            {
                Console.WriteLine("❌ Cannot remove client with existing orders!");
                return;
            }

            string deleteQuery = "DELETE FROM Clients WHERE id_Client = @id";
            using var deleteCmd = new NpgsqlCommand(deleteQuery, connection);
            deleteCmd.Parameters.AddWithValue("@id", clientId);

            int affected = deleteCmd.ExecuteNonQuery();
            Console.WriteLine(affected > 0 ? "✅ Client removed!" : "❌ Client not found!");
        }
    }
}
