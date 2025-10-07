using Electronics_Shop2.Data;
using Npgsql;

namespace Electronics_Shop2.Managers
{
    public class StaffManager : BaseManager
    {
        // NO CONSTRUCTOR - uses BaseManager's constructor automatically

        public void ShowStaffManagement()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("👨‍💼 STAFF MANAGEMENT");
                Console.WriteLine("1. View All Staff");
                Console.WriteLine("2. Add New Staff");
                Console.WriteLine("3. Update Staff");
                Console.WriteLine("4. Remove Staff");
                Console.WriteLine("5. Back to Main Menu");
                Console.Write("Choose option: ");

                try
                {
                    EnsureConnectionOpen();

                    switch (Console.ReadLine())
                    {
                        case "1": ViewAllStaff(); break;
                        case "2": AddNewStaff(); break;
                        case "3": UpdateStaff(); break;
                        case "4": RemoveStaff(); break;
                        case "5": return;
                        default: Console.WriteLine("Invalid option!"); break;
                    }
                }
                catch (Exception ex)
                {
                    HandleException(ex, "staff management");
                }
                finally
                {
                    EnsureConnectionClosed();
                }

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }



        public void ViewAllStaff()
        {
            Console.WriteLine("\n=== All Staff ===");
            string query = @"SELECT s.id_Staff, s.Staff_Name, p.Position, s.Salary, s.Hire_Date, s.Phone_Number
                        FROM Staff s JOIN Positions p ON s.id_Position = p.id_Position
                        ORDER BY s.Staff_Name";

            using var command = new NpgsqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader["id_Staff"]} | Name: {reader["Staff_Name"]} | Position: {reader["Position"]}");
                Console.WriteLine($"   Salary: ${reader["Salary"]} | Phone: {reader["Phone_Number"]} | Hired: {reader["Hire_Date"]:yyyy-MM-dd}");
                Console.WriteLine("----------------------------------------");
            }
        }

        public void ViewPositions()
        {
            Console.WriteLine("\n=== Available Positions ===");
            string query = "SELECT id_Position, Position, Salary FROM Positions";
            using var command = new NpgsqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader["id_Position"]} | {reader["Position"]} (${reader["Salary"]})");
            }
        }

        public int AddNewStaff()
        {
            ViewPositions();
            Console.Write("Enter position ID: ");
            if (!int.TryParse(Console.ReadLine(), out int positionId))
            {
                Console.WriteLine("❌ Invalid position ID!");
                return -1;
            }

            Console.Write("Enter staff name: ");
            string name = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("❌ Staff name cannot be empty!");
                return -1;
            }

            Console.Write("Enter phone number: ");
            string phone = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrEmpty(phone))
            {
                Console.WriteLine("❌ Phone number cannot be empty!");
                return -1;
            }

            Console.Write("Enter salary: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal salary) || salary <= 0)
            {
                Console.WriteLine("❌ Invalid salary!");
                return -1;
            }

            string query = @"INSERT INTO Staff (Staff_Name, id_Position, Phone_Number, Salary, Hire_Date) 
            VALUES (@name, @position, @phone, @salary, @hireDate)
            RETURNING id_Staff;";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@position", positionId);
            command.Parameters.AddWithValue("@phone", phone);
            command.Parameters.AddWithValue("@salary", salary);
            command.Parameters.AddWithValue("@hireDate", DateTime.Today);

            int newId = Convert.ToInt32(command.ExecuteScalar());
            Console.WriteLine($"✅ Staff '{name}' added with ID: {newId}");
            return newId;
        }

        public void UpdateStaff()
        {
            try
            {
                EnsureConnectionOpen();

                ViewAllStaff();
                Console.Write("\nEnter staff ID to update: ");
                if (!int.TryParse(Console.ReadLine(), out int staffId)) return;

                // Check if staff exists
                string checkQuery = "SELECT Staff_Name FROM Staff WHERE id_Staff = @id";
                using var checkCmd = new NpgsqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@id", staffId);
                var staffName = checkCmd.ExecuteScalar()?.ToString();

                if (staffName == null)
                {
                    Console.WriteLine("❌ Staff not found!");
                    return;
                }

                Console.WriteLine($"\nUpdating staff: {staffName} (ID: {staffId})");
                Console.WriteLine("What would you like to update?");
                Console.WriteLine("1. Name");
                Console.WriteLine("2. Position");
                Console.WriteLine("3. Phone Number");
                Console.WriteLine("4. Salary");
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
                        updateQuery = "UPDATE Staff SET Staff_Name = @name WHERE id_Staff = @id";
                        using (var cmd = new NpgsqlCommand(updateQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@name", newName);
                            cmd.Parameters.AddWithValue("@id", staffId);
                            affected = cmd.ExecuteNonQuery();
                        }
                        break;

                    case "2": // Update Position
                        ViewPositions();
                        Console.Write("Enter new position ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int newPositionId))
                        {
                            Console.WriteLine("❌ Invalid position ID!");
                            return;
                        }
                        updateQuery = "UPDATE Staff SET id_Position = @position WHERE id_Staff = @id";
                        using (var cmd = new NpgsqlCommand(updateQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@position", newPositionId);
                            cmd.Parameters.AddWithValue("@id", staffId);
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
                        updateQuery = "UPDATE Staff SET Phone_Number = @phone WHERE id_Staff = @id";
                        using (var cmd = new NpgsqlCommand(updateQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@phone", newPhone);
                            cmd.Parameters.AddWithValue("@id", staffId);
                            affected = cmd.ExecuteNonQuery();
                        }
                        break;

                    case "4": // Update Salary
                        Console.Write("Enter new salary: ");
                        if (!decimal.TryParse(Console.ReadLine(), out decimal newSalary) || newSalary <= 0)
                        {
                            Console.WriteLine("❌ Invalid salary!");
                            return;
                        }
                        updateQuery = "UPDATE Staff SET Salary = @salary WHERE id_Staff = @id";
                        using (var cmd = new NpgsqlCommand(updateQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@salary", newSalary);
                            cmd.Parameters.AddWithValue("@id", staffId);
                            affected = cmd.ExecuteNonQuery();
                        }
                        break;

                    case "5": // Update Everything
                        ViewPositions();
                        Console.Write("Enter new position ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int positionId))
                        {
                            Console.WriteLine("❌ Invalid position ID!");
                            return;
                        }
                        Console.Write("Enter new name: ");
                        string name = Console.ReadLine()?.Trim() ?? "";
                        if (string.IsNullOrEmpty(name))
                        {
                            Console.WriteLine("❌ Name cannot be empty!");
                            return;
                        }
                        Console.Write("Enter new phone: ");
                        string phone = Console.ReadLine()?.Trim() ?? "";
                        if (string.IsNullOrEmpty(phone))
                        {
                            Console.WriteLine("❌ Phone number cannot be empty!");
                            return;
                        }
                        Console.Write("Enter new salary: ");
                        if (!decimal.TryParse(Console.ReadLine(), out decimal salary) || salary <= 0)
                        {
                            Console.WriteLine("❌ Invalid salary!");
                            return;
                        }

                        updateQuery = @"UPDATE Staff SET Staff_Name = @name, id_Position = @position, 
                            Phone_Number = @phone, Salary = @salary WHERE id_Staff = @id";
                        using (var cmd = new NpgsqlCommand(updateQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@name", name);
                            cmd.Parameters.AddWithValue("@position", positionId);
                            cmd.Parameters.AddWithValue("@phone", phone);
                            cmd.Parameters.AddWithValue("@salary", salary);
                            cmd.Parameters.AddWithValue("@id", staffId);
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

                Console.WriteLine(affected > 0 ? "✅ Staff updated successfully!" : "❌ No changes made!");
            }
            catch (Exception ex)
            {
                HandleException(ex, "updating staff");
            }
        }


        public void RemoveStaff()
        {
            ViewAllStaff();
            Console.Write("\nEnter staff ID to remove: ");
            if (!int.TryParse(Console.ReadLine(), out int staffId)) return;

            // Check for existing orders
            string checkQuery = "SELECT COUNT(*) FROM Orders WHERE id_Staff = @id";
            using var checkCmd = new NpgsqlCommand(checkQuery, connection);
            checkCmd.Parameters.AddWithValue("@id", staffId);

            if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
            {
                Console.WriteLine("❌ Cannot remove staff with processed orders!");
                return;
            }

            string deleteQuery = "DELETE FROM Staff WHERE id_Staff = @id";
            using var deleteCmd = new NpgsqlCommand(deleteQuery, connection);
            deleteCmd.Parameters.AddWithValue("@id", staffId);

            int affected = deleteCmd.ExecuteNonQuery();
            Console.WriteLine(affected > 0 ? "✅ Staff removed!" : "❌ Staff not found!");
        }
    }
}
