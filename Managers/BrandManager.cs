using Electronics_Shop2.Data;
using Npgsql;

namespace Electronics_Shop2.Managers
{
    public class BrandManager : BaseManager
    {
        public void ShowBrandManagementMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("🏷️ BRAND MANAGEMENT");
                Console.WriteLine("1. View All Brands");
                Console.WriteLine("2. Add New Brand");
                Console.WriteLine("3. Update Brand");
                Console.WriteLine("4. Delete Brand");
                Console.WriteLine("5. Back to Main Menu");
                Console.Write("Choose option: ");

                var choice = Console.ReadLine();

                try
                {
                    // Don't open connection here - let each method handle its own connection
                    switch (choice)
                    {
                        case "1": ViewAllBrands(); break;
                        case "2": AddNewBrand(); break;
                        case "3": UpdateBrand(); break;
                        case "4": DeleteBrand(); break;
                        case "5": return;
                        default: Console.WriteLine("Invalid option!"); break;
                    }
                }
                catch (Exception ex)
                {
                    HandleException(ex, "brand management");
                }

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }

        public void ViewAllBrands()
        {
            try
            {
                EnsureConnectionOpen();

                Console.WriteLine("\n=== All Brands ===");
                string query = "SELECT id_Brand, Brand_Name FROM Brands ORDER BY Brand_Name";

                using var command = new NpgsqlCommand(query, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Console.WriteLine($"ID: {reader["id_Brand"]} | Name: {reader["Brand_Name"]}");
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "viewing brands");
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        public int AddNewBrand()
        {
            try
            {
                EnsureConnectionOpen();

                Console.Write("Enter brand name: ");
                string brandName = Console.ReadLine()?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(brandName))
                {
                    Console.WriteLine("Brand name cannot be empty!");
                    return -1;
                }

                // Check if brand already exists
                string checkQuery = "SELECT id_Brand FROM Brands WHERE LOWER(Brand_Name) = LOWER(@name)";
                using var checkCmd = new NpgsqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@name", brandName);

                var existingId = checkCmd.ExecuteScalar();
                if (existingId != null)
                {
                    Console.WriteLine($"Brand '{brandName}' already exists with ID: {existingId}");
                    return Convert.ToInt32(existingId);
                }

                // Insert new brand - FIXED: Removed semicolon before RETURNING
                string insertQuery = "INSERT INTO Brands (Brand_Name) VALUES (@name) RETURNING id_Brand";
                using var insertCmd = new NpgsqlCommand(insertQuery, connection);
                insertCmd.Parameters.AddWithValue("@name", brandName);

                var result = insertCmd.ExecuteScalar();
                if (result != null)
                {
                    int newBrandId = Convert.ToInt32(result);
                    Console.WriteLine($"✅ Brand '{brandName}' added successfully with ID: {newBrandId}");
                    return newBrandId;
                }
                else
                {
                    Console.WriteLine("❌ Failed to add brand - no ID returned");
                    return -1;
                }
            }
            catch (PostgresException pgEx)
            {
                Console.WriteLine($"❌ Database error: {pgEx.MessageText}");
                Console.WriteLine($"SQL State: {pgEx.SqlState}");
                return -1;
            }
            catch (Exception ex)
            {
                HandleException(ex, "adding brand");
                return -1;
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        public void UpdateBrand()
        {
            try
            {
                EnsureConnectionOpen();

                ViewAllBrands();
                Console.Write("\nEnter brand ID to update: ");
                if (!int.TryParse(Console.ReadLine(), out int brandId))
                {
                    Console.WriteLine("Invalid ID!");
                    return;
                }

                Console.Write("Enter new brand name: ");
                string newName = Console.ReadLine()?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(newName))
                {
                    Console.WriteLine("Brand name cannot be empty!");
                    return;
                }

                // Check if new name already exists (excluding current brand)
                string checkQuery = "SELECT id_Brand FROM Brands WHERE LOWER(Brand_Name) = LOWER(@name) AND id_Brand != @id";
                using var checkCmd = new NpgsqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@name", newName);
                checkCmd.Parameters.AddWithValue("@id", brandId);

                var existingId = checkCmd.ExecuteScalar();
                if (existingId != null)
                {
                    Console.WriteLine($"Brand '{newName}' already exists with ID: {existingId}");
                    return;
                }

                string query = "UPDATE Brands SET Brand_Name = @name WHERE id_Brand = @id";
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@name", newName);
                command.Parameters.AddWithValue("@id", brandId);

                int affected = command.ExecuteNonQuery();
                if (affected > 0)
                    Console.WriteLine("✅ Brand updated successfully!");
                else
                    Console.WriteLine("❌ Brand not found!");
            }
            catch (Exception ex)
            {
                HandleException(ex, "updating brand");
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        public void DeleteBrand()
        {
            try
            {
                EnsureConnectionOpen();

                ViewAllBrands();
                Console.Write("\nEnter brand ID to delete: ");
                if (!int.TryParse(Console.ReadLine(), out int brandId))
                {
                    Console.WriteLine("Invalid ID!");
                    return;
                }

                // Check if brand has models
                string checkQuery = @"SELECT COUNT(*) FROM Phone_Models WHERE id_Brand = @id";
                using var checkCmd = new NpgsqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@id", brandId);

                int modelCount = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (modelCount > 0)
                {
                    Console.WriteLine($"❌ Cannot delete brand! It has {modelCount} models associated.");
                    return;
                }

                string deleteQuery = "DELETE FROM Brands WHERE id_Brand = @id";
                using var deleteCmd = new NpgsqlCommand(deleteQuery, connection);
                deleteCmd.Parameters.AddWithValue("@id", brandId);

                int affected = deleteCmd.ExecuteNonQuery();
                if (affected > 0)
                    Console.WriteLine("✅ Brand deleted successfully!");
                else
                    Console.WriteLine("❌ Brand not found!");
            }
            catch (Exception ex)
            {
                HandleException(ex, "deleting brand");
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        // Helper method to get brand name by ID (useful for other managers)
        public string GetBrandNameById(int brandId)
        {
            try
            {
                EnsureConnectionOpen();

                string query = "SELECT Brand_Name FROM Brands WHERE id_Brand = @id";
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", brandId);

                var result = command.ExecuteScalar();
                return result?.ToString() ?? "Unknown Brand";
            }
            catch (Exception ex)
            {
                HandleException(ex, "getting brand name");
                return "Unknown Brand";
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }
    }
}