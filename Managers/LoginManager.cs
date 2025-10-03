using Electronics_Shop2.Data;
using Npgsql;

namespace Electronics_Shop2.Managers
{
    public class LoginManager : BaseManager
    {
        // ✅ Make methods static so they can be called without creating an instance
        public static (int staffId, string staffName, bool success) StaffLogin()
        {
            var loginManager = new LoginManager(); // Create instance internally

            Console.Clear();
            Console.WriteLine("🏪 ELECTRONICS SHOP - STAFF LOGIN");
            Console.WriteLine("=================================\n");

            try
            {
                loginManager.EnsureConnectionOpen();

                // Show available staff
                Console.WriteLine("Available Staff:\n");
                string staffQuery = @"SELECT s.id_Staff, s.Staff_Name, p.Position 
                                    FROM Staff s 
                                    JOIN Positions p ON s.id_Position = p.id_Position 
                                    ORDER BY s.Staff_Name";

                using var command = new NpgsqlCommand(staffQuery, loginManager.connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Console.WriteLine($"ID: {reader["id_Staff"]} | {reader["Staff_Name"]} - {reader["Position"]}");
                }
                reader.Close();

                Console.Write("\nEnter your Staff ID: ");
                if (!int.TryParse(Console.ReadLine(), out int staffId))
                {
                    Console.WriteLine("❌ Invalid Staff ID!");
                    return (0, "", false);
                }

                // Verify staff exists
                string verifyQuery = "SELECT Staff_Name FROM Staff WHERE id_Staff = @id";
                using var verifyCmd = new NpgsqlCommand(verifyQuery, loginManager.connection);
                verifyCmd.Parameters.AddWithValue("@id", staffId);

                var result = verifyCmd.ExecuteScalar();
                if (result == null)
                {
                    Console.WriteLine("❌ Staff ID not found!");
                    return (0, "", false);
                }

                string staffName = result.ToString() ?? "";

                Console.WriteLine($"\n✅ Welcome, {staffName}!");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();

                return (staffId, staffName, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Login error: {ex.Message}");
                return (0, "", false);
            }
            finally
            {
                loginManager.EnsureConnectionClosed();
            }
        }

        public static bool SwitchStaff(out int newStaffId, out string newStaffName)
        {
            newStaffId = 0;
            newStaffName = "";

            Console.Clear();
            Console.WriteLine("🔄 SWITCH STAFF");
            Console.WriteLine("===============\n");

            Console.Write("Do you want to switch staff? (y/n): ");
            var response = Console.ReadLine()?.ToLower();

            if (response == "y" || response == "yes" || response == "ye" || response == "da" || response == "да")
            {
                var result = StaffLogin();
                if (result.success)
                {
                    newStaffId = result.staffId;
                    newStaffName = result.staffName;
                    return true;
                }
            }
            return false;
        }
    }
}