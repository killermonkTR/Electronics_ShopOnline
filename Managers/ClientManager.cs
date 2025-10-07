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
            string name = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("❌ Client name cannot be empty!");
                return -1;
            }

            Console.Write("Enter email: ");
            string email = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrEmpty(email))
            {
                Console.WriteLine("❌ Email cannot be empty!");
                return -1;
            }

            Console.Write("Enter phone number: ");
            string phone = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrEmpty(phone))
            {
                Console.WriteLine("❌ Phone number cannot be empty!");
                return -1;
            }

            Console.Write("Enter address: ");
            string address = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrEmpty(address))
            {
                Console.WriteLine("❌ Address cannot be empty!");
                return -1;
            }

            string query = @"INSERT INTO Clients (Name, Email, Phone_Number, Address, Registration_Date) 
             VALUES (@name, @email, @phone, @address, @regDate)
             RETURNING id_Client;";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@phone", phone);
            command.Parameters.AddWithValue("@address", address);
            command.Parameters.AddWithValue("@regDate", DateTime.Today);

            int newId = Convert.ToInt32(command.ExecuteScalar());
            Console.WriteLine($"✅ Client '{name}' added with ID: {newId}");
            return newId;
        }

        public void UpdateClient()
        {
            try
            {
                EnsureConnectionOpen();

                ViewAllClients();
                Console.Write("\nEnter client ID to update: ");
                if (!int.TryParse(Console.ReadLine(), out int clientId)) return;

                // Check if client exists
                string checkQuery = "SELECT Name FROM Clients WHERE id_Client = @id";
                using var checkCmd = new NpgsqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@id", clientId);
                var clientName = checkCmd.ExecuteScalar()?.ToString();

                if (clientName == null)
                {
                    Console.WriteLine("❌ Client not found!");
                    return;
                }

                Console.WriteLine($"\nUpdating client: {clientName} (ID: {clientId})");
                Console.WriteLine("What would you like to update?");
                Console.WriteLine("1. Name");
                Console.WriteLine("2. Email");
                Console.WriteLine("3. Phone Number");
                Console.WriteLine("4. Address");
                Console.WriteLine("5. Update Everything");
                Console.WriteLine("6. Cancel");
                Console.Write("Choose option: ");

                var choice = Console.ReadLine();
                string updateQuery = "";
                int affected = 0;

                switch (choice)
                {
                    case "1": // Update Name
                        Console.Write("Enter new name: ");
                        string newName = Console.ReadLine()?.Trim() ?? "";
                        if (string.IsNullOrEmpty(newName))
                        {
                            Console.WriteLine("❌ Name cannot be empty!");
                            return;
                        }
                        updateQuery = "UPDATE Clients SET Name = @name WHERE id_Client = @id";
                        using (var cmd = new NpgsqlCommand(updateQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@name", newName);
                            cmd.Parameters.AddWithValue("@id", clientId);
                            affected = cmd.ExecuteNonQuery();
                        }
                        break;

                    case "2": // Update Email
                        Console.Write("Enter new email: ");
                        string newEmail = Console.ReadLine()?.Trim() ?? "";
                        if (string.IsNullOrEmpty(newEmail))
                        {
                            Console.WriteLine("❌ Email cannot be empty!");
                            return;
                        }
                        updateQuery = "UPDATE Clients SET Email = @email WHERE id_Client = @id";
                        using (var cmd = new NpgsqlCommand(updateQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@email", newEmail);
                            cmd.Parameters.AddWithValue("@id", clientId);
                            affected = cmd.ExecuteNonQuery();
                        }
                        break;

                    case "3": // Update Phone
                        Console.Write("Enter new phone number: ");
                        string newPhone = Console.ReadLine()?.Trim() ?? "";
                        if (string.IsNullOrEmpty(newPhone))
                        {
                            Console.WriteLine("❌ Phone number cannot be empty!");
                            return;
                        }
                        updateQuery = "UPDATE Clients SET Phone_Number = @phone WHERE id_Client = @id";
                        using (var cmd = new NpgsqlCommand(updateQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@phone", newPhone);
                            cmd.Parameters.AddWithValue("@id", clientId);
                            affected = cmd.ExecuteNonQuery();
                        }
                        break;

                    case "4": // Update Address
                        Console.Write("Enter new address: ");
                        string newAddress = Console.ReadLine()?.Trim() ?? "";
                        if (string.IsNullOrEmpty(newAddress))
                        {
                            Console.WriteLine("❌ Address cannot be empty!");
                            return;
                        }
                        updateQuery = "UPDATE Clients SET Address = @address WHERE id_Client = @id";
                        using (var cmd = new NpgsqlCommand(updateQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@address", newAddress);
                            cmd.Parameters.AddWithValue("@id", clientId);
                            affected = cmd.ExecuteNonQuery();
                        }
                        break;

                    case "5": // Update Everything
                        Console.Write("Enter new name: ");
                        string name = Console.ReadLine()?.Trim() ?? "";
                        if (string.IsNullOrEmpty(name))
                        {
                            Console.WriteLine("❌ Name cannot be empty!");
                            return;
                        }
                        Console.Write("Enter new email: ");
                        string email = Console.ReadLine()?.Trim() ?? "";
                        if (string.IsNullOrEmpty(email))
                        {
                            Console.WriteLine("❌ Email cannot be empty!");
                            return;
                        }
                        Console.Write("Enter new phone: ");
                        string phone = Console.ReadLine()?.Trim() ?? "";
                        if (string.IsNullOrEmpty(phone))
                        {
                            Console.WriteLine("❌ Phone number cannot be empty!");
                            return;
                        }
                        Console.Write("Enter new address: ");
                        string address = Console.ReadLine()?.Trim() ?? "";
                        if (string.IsNullOrEmpty(address))
                        {
                            Console.WriteLine("❌ Address cannot be empty!");
                            return;
                        }

                        updateQuery = @"UPDATE Clients SET Name = @name, Email = @email, 
                            Phone_Number = @phone, Address = @address 
                            WHERE id_Client = @id";
                        using (var cmd = new NpgsqlCommand(updateQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@name", name);
                            cmd.Parameters.AddWithValue("@email", email);
                            cmd.Parameters.AddWithValue("@phone", phone);
                            cmd.Parameters.AddWithValue("@address", address);
                            cmd.Parameters.AddWithValue("@id", clientId);
                            affected = cmd.ExecuteNonQuery();
                        }
                        break;

                    case "6": // Cancel
                        Console.WriteLine("Update cancelled!");
                        return;

                    default:
                        Console.WriteLine("❌ Invalid option!");
                        return;
                }

                Console.WriteLine(affected > 0 ? "✅ Client updated successfully!" : "❌ No changes made!");
            }
            catch (Exception ex)
            {
                HandleException(ex, "updating client");
            }
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
