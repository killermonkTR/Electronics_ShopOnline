using Electronics_Shop2.Data;
using Electronics_Shop2.Models;
using Electronics_Shop2.Managers;
using Npgsql;


namespace Electronics_Shop2.Managers
{
    public class ModelManager : BaseManager
    {
      
        public void ShowModelManagementMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("📱 MODEL MANAGEMENT");
                Console.WriteLine("1. View All Models");
                Console.WriteLine("2. Add New Model");
                Console.WriteLine("3. Update Model");
                Console.WriteLine("4. Delete Model");
                Console.WriteLine("5. Back to Main Menu");
                Console.Write("Choose option: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": ViewAllModels(); break;
                    case "2": AddNewModel(); break;
                    case "3": UpdateModel(); break;
                    case "4": DeleteModel(); break;
                    case "5": return;
                    default: Console.WriteLine("Invalid option!"); break;
                }

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }

        public void ViewAllModels()
        {
            Console.WriteLine("\n=== All Models ===");
            string query = @"
            SELECT pm.id_Model, pm.Phone_Model, b.Brand_Name, b.id_Brand
            FROM Phone_Models pm
            JOIN Brands b ON pm.id_Brand = b.id_Brand
            ORDER BY b.Brand_Name, pm.Phone_Model";

            using var command = new NpgsqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader["id_Model"]} | Model: {reader["Phone_Model"]} | Brand: {reader["Brand_Name"]}");
            }
        }

        public int AddNewModel()
        {
            EnsureConnectionOpen();
            // Show available brands
            Console.WriteLine("\n=== Available Brands ===");
            string brandQuery = "SELECT id_Brand, Brand_Name FROM Brands ORDER BY Brand_Name";
            using var brandCmd = new NpgsqlCommand(brandQuery, connection);
            using var brandReader = brandCmd.ExecuteReader();

            while (brandReader.Read())
            {
                Console.WriteLine($"ID: {brandReader["id_Brand"]} | {brandReader["Brand_Name"]}");
            }
            brandReader.Close();

            Console.Write("Enter brand ID: ");
            if (!int.TryParse(Console.ReadLine(), out int brandId))
            {
                Console.WriteLine("Invalid brand ID!");
                return -1;
            }

            Console.Write("Enter model name: ");
            string modelName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(modelName))
            {
                Console.WriteLine("Model name cannot be empty!");
                return -1;
            }

            // Check if model already exists for this brand
            string checkQuery = "SELECT id_Model FROM Phone_Models WHERE Phone_Model = @model AND id_Brand = @brandId";
            using var checkCmd = new NpgsqlCommand(checkQuery, connection);
            checkCmd.Parameters.AddWithValue("@model", modelName);
            checkCmd.Parameters.AddWithValue("@brandId", brandId);

            var existingId = checkCmd.ExecuteScalar();
            if (existingId != null)
            {
                Console.WriteLine($"Model '{modelName}' already exists for this brand with ID: {existingId}");
                return Convert.ToInt32(existingId);
            }

            // Insert new model
            string insertQuery = "INSERT INTO Phone_Models (Phone_Model, id_Brand) VALUES (@model, @brandId); SELECT LAST_INSERT_ID();";
            using var insertCmd = new NpgsqlCommand(insertQuery, connection);
            insertCmd.Parameters.AddWithValue("@model", modelName);
            insertCmd.Parameters.AddWithValue("@brandId", brandId);

            int newModelId = Convert.ToInt32(insertCmd.ExecuteScalar());
            Console.WriteLine($"✅ Model '{modelName}' added successfully with ID: {newModelId}");
            return newModelId;
        }

        public void UpdateModel()
        {
            ViewAllModels();
            Console.Write("\nEnter model ID to update: ");
            if (!int.TryParse(Console.ReadLine(), out int modelId))
            {
                Console.WriteLine("Invalid ID!");
                return;
            }

            Console.Write("Enter new model name: ");
            string newName = Console.ReadLine();

            // Show brands for reassignment
            Console.WriteLine("\n=== Available Brands ===");
            string brandQuery = "SELECT id_Brand, Brand_Name FROM Brands ORDER BY Brand_Name";
            using var brandCmd = new NpgsqlCommand(brandQuery, connection);
            using var brandReader = brandCmd.ExecuteReader();

            while (brandReader.Read())
            {
                Console.WriteLine($"ID: {brandReader["id_Brand"]} | {brandReader["Brand_Name"]}");
            }
            brandReader.Close();

            Console.Write("Enter new brand ID: ");
            if (!int.TryParse(Console.ReadLine(), out int newBrandId))
            {
                Console.WriteLine("Invalid brand ID!");
                return;
            }

            string query = "UPDATE Phone_Models SET Phone_Model = @name, id_Brand = @brandId WHERE id_Model = @id";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", newName);
            command.Parameters.AddWithValue("@brandId", newBrandId);
            command.Parameters.AddWithValue("@id", modelId);

            int affected = command.ExecuteNonQuery();
            if (affected > 0)
                Console.WriteLine("✅ Model updated successfully!");
            else
                Console.WriteLine("❌ Model not found!");
        }

        public void DeleteModel()
        {
            ViewAllModels();
            Console.Write("\nEnter model ID to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int modelId))
            {
                Console.WriteLine("Invalid ID!");
                return;
            }

            // Check if model has products
            string checkQuery = "SELECT COUNT(*) FROM Products WHERE id_Model = @id";
            using var checkCmd = new NpgsqlCommand(checkQuery, connection);
            checkCmd.Parameters.AddWithValue("@id", modelId);

            int productCount = Convert.ToInt32(checkCmd.ExecuteScalar());
            if (productCount > 0)
            {
                Console.WriteLine($"❌ Cannot delete model! It has {productCount} products associated.");
                return;
            }

            string deleteQuery = "DELETE FROM Phone_Models WHERE id_Model = @id";
            using var deleteCmd = new NpgsqlCommand(deleteQuery, connection);
            deleteCmd.Parameters.AddWithValue("@id", modelId);

            int affected = deleteCmd.ExecuteNonQuery();
            if (affected > 0)
                Console.WriteLine("✅ Model deleted successfully!");
            else
                Console.WriteLine("❌ Model not found!");
        }
    }
}
