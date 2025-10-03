using Electronics_Shop2.Data;
using Electronics_Shop2.Models;
using Electronics_Shop2.Managers;
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
            if (!int.TryParse(Console.ReadLine(), out int positionId)) return -1;

            Console.Write("Enter staff name: ");
            string name = Console.ReadLine();
            Console.Write("Enter phone number: ");
            string phone = Console.ReadLine();
            Console.Write("Enter salary: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal salary)) return -1;

            string query = @"INSERT INTO Staff (Staff_Name, id_Position, Phone_Number, Salary, Hire_Date) 
            VALUES (@name, @position, @phone, @salary, @hireDate)
            RETURNING id_Staff;";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@position", positionId);
            command.Parameters.AddWithValue("@phone", string.IsNullOrEmpty(phone) ? DBNull.Value : phone);
            command.Parameters.AddWithValue("@salary", salary);
            command.Parameters.AddWithValue("@hireDate", DateTime.Today); // Add this parameter

            int newId = Convert.ToInt32(command.ExecuteScalar());
            Console.WriteLine($"✅ Staff '{name}' added with ID: {newId}");
            return newId;
        }

        public void UpdateStaff()
        {
            ViewAllStaff();
            Console.Write("\nEnter staff ID to update: ");
            if (!int.TryParse(Console.ReadLine(), out int staffId)) return;

            ViewPositions();
            Console.Write("Enter new position ID: ");
            if (!int.TryParse(Console.ReadLine(), out int positionId)) return;

            Console.Write("Enter new name: ");
            string name = Console.ReadLine();
            Console.Write("Enter new phone: ");
            string phone = Console.ReadLine();
            Console.Write("Enter new salary: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal salary)) return;

            string query = @"UPDATE Staff SET Staff_Name = @name, id_Position = @position, 
                        Phone_Number = @phone, Salary = @salary WHERE id_Staff = @id";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@position", positionId);
            command.Parameters.AddWithValue("@phone", string.IsNullOrEmpty(phone) ? DBNull.Value : phone);
            command.Parameters.AddWithValue("@salary", salary);
            command.Parameters.AddWithValue("@id", staffId);

            int affected = command.ExecuteNonQuery();
            Console.WriteLine(affected > 0 ? "✅ Staff updated!" : "❌ Staff not found!");
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
