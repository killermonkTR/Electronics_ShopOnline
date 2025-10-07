using Electronics_Shop2.Data;
using Npgsql;

namespace Electronics_Shop2.Managers
{
    public class ProductManager : BaseManager
    {
        public void ShowProductManagement()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("📦 PRODUCT MANAGEMENT");
                Console.WriteLine("1. View All Products");
                Console.WriteLine("2. Add New Product");
                Console.WriteLine("3. Update Product");
                Console.WriteLine("4. Remove Product");
                Console.WriteLine("5. Back to Main Menu");

                try
                {
                    EnsureConnectionOpen();

                    switch (Console.ReadLine())
                    {
                        case "1": ViewAllProducts(); break;
                        case "2": AddNewProduct(); break;
                        case "3": UpdateProduct(); break;
                        case "4": RemoveProduct(); break;
                        case "5": return;
                        default: Console.WriteLine("Invalid option!"); break;
                    }
                }
                catch (Exception ex)
                {
                    HandleException(ex, "product management");
                }
                finally
                {
                    EnsureConnectionClosed();
                }

                Console.ReadKey();
            }
        }


        public void ViewAllProducts()
        {
            Console.WriteLine("\n=== All Products ===");
            string query = @"SELECT p.id_Product, p.Product_Name, c.Category_Name, b.Brand_Name, 
                    pm.Phone_Model, p.Price, p.Stock_Quantity, p.Warranty
                    FROM Products p
                    JOIN Categories c ON p.id_Category = c.id_Category
                    JOIN Phone_Models pm ON p.id_Model = pm.id_Model
                    JOIN Brands b ON pm.id_Brand = b.id_Brand
                    ORDER BY p.Product_Name";

            using var command = new NpgsqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader["id_Product"]} | {GetSafeString(reader["Product_Name"])}");
                Console.WriteLine($"   Category: {GetSafeString(reader["Category_Name"])} | Brand: {GetSafeString(reader["Brand_Name"])} | Model: {GetSafeString(reader["Phone_Model"])}");
                Console.WriteLine($"   Price: ${GetSafeDecimal(reader["Price"])} | Stock: {GetSafeInt(reader["Stock_Quantity"])} | Warranty: {GetSafeInt(reader["Warranty"])} months");
                Console.WriteLine("----------------------------------------");
            }
        }

        public void ViewCategories()
        {
            Console.WriteLine("\n=== Available Categories ===");
            string query = "SELECT id_Category, Category_Name FROM Categories";
            using var command = new NpgsqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader["id_Category"]} | {GetSafeString(reader["Category_Name"])}");
            }
        }


        public int AddNewProduct()
        {
            try
            {
                EnsureConnectionOpen();
                Console.Clear();
                Console.WriteLine("📦 ADD NEW PRODUCT");
                Console.WriteLine("==================\n");

                // Step 1: Select Brand
                Console.WriteLine("=== Available Brands ===");
                string brandQuery = "SELECT id_Brand, Brand_Name FROM Brands";
                using var brandCmd = new NpgsqlCommand(brandQuery, connection);
                using var brandReader = brandCmd.ExecuteReader();

                while (brandReader.Read())
                {
                    Console.WriteLine($"ID: {brandReader["id_Brand"]} | {GetSafeString(brandReader["Brand_Name"])}");
                }
                brandReader.Close();

                Console.Write("Enter brand ID: ");
                if (!int.TryParse(Console.ReadLine(), out int brandId)) return -1;

                // Step 2: Show models for selected brand
                Console.WriteLine($"\n=== Available Models for Brand ID {brandId} ===");
                string modelQuery = "SELECT id_Model, Phone_Model FROM Phone_Models WHERE id_Brand = @brandId";
                using var modelCmd = new NpgsqlCommand(modelQuery, connection);
                modelCmd.Parameters.AddWithValue("@brandId", brandId);
                using var modelReader = modelCmd.ExecuteReader();

                bool hasModels = false;
                while (modelReader.Read())
                {
                    hasModels = true;
                    Console.WriteLine($"ID: {modelReader["id_Model"]} | {GetSafeString(modelReader["Phone_Model"])}");
                }
                modelReader.Close();

                if (!hasModels)
                {
                    Console.WriteLine("❌ No models found for this brand! Please add models first.");
                    return -1;
                }

                Console.Write("Enter model ID: ");
                if (!int.TryParse(Console.ReadLine(), out int modelId)) return -1;

                // Step 3: Select Category
                Console.WriteLine("\n=== Available Categories ===");
                ViewCategories();
                Console.Write("Enter category ID: ");
                if (!int.TryParse(Console.ReadLine(), out int categoryId)) return -1;

                // Step 4: Get product details
                Console.Write("Enter product name: ");
                string name = Console.ReadLine() ?? "";
                Console.Write("Enter price: ");
                if (!decimal.TryParse(Console.ReadLine(), out decimal price)) return -1;
                Console.Write("Enter stock quantity: ");
                if (!int.TryParse(Console.ReadLine(), out int stock)) return -1;
                Console.Write("Enter warranty months: ");
                if (!int.TryParse(Console.ReadLine(), out int warranty)) warranty = 0;

                // Step 5: Insert product
                string insertQuery = @"INSERT INTO Products (Product_Name, id_Category, id_Model, Warranty, Price, Stock_Quantity) 
                        VALUES (@name, @category, @model, @warranty, @price, @stock)
                        RETURNING id_Product;";

                using var command = new NpgsqlCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@category", categoryId);
                command.Parameters.AddWithValue("@model", modelId);
                command.Parameters.AddWithValue("@warranty", warranty);
                command.Parameters.AddWithValue("@price", price);
                command.Parameters.AddWithValue("@stock", stock);

                int newId = Convert.ToInt32(command.ExecuteScalar());
                Console.WriteLine($"✅ Product '{name}' added with ID: {newId}");
                return newId;
            }
            catch (Exception ex)
            {
                HandleException(ex, "adding product");
                return -1;
            }
        }


        public void UpdateProduct()
        {
            try
            {
                EnsureConnectionOpen();

                ViewAllProducts();
                Console.Write("\nEnter product ID to update: ");
                if (!int.TryParse(Console.ReadLine(), out int productId)) return;

                // Check if product exists
                string checkQuery = "SELECT Product_Name FROM Products WHERE id_Product = @id";
                using var checkCmd = new NpgsqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@id", productId);
                var productName = checkCmd.ExecuteScalar()?.ToString();

                if (productName == null)
                {
                    Console.WriteLine("❌ Product not found!");
                    return;
                }

                Console.WriteLine($"\nUpdating product: {productName} (ID: {productId})");
                Console.WriteLine("What would you like to update?");
                Console.WriteLine("1. Product Name");
                Console.WriteLine("2. Category");
                Console.WriteLine("3. Price");
                Console.WriteLine("4. Stock Quantity");
                Console.WriteLine("5. Warranty");
                Console.WriteLine("6. Update Everything");
                Console.WriteLine("7. Cancel");
                Console.Write("Choose option: ");

                var choice = Console.ReadLine();
                string updateQuery = "";
                int affected = 0;

                switch (choice)
                {
                    case "1": // Update Name
                        Console.Write("Enter new product name: ");
                        string newName = Console.ReadLine() ?? "";
                        updateQuery = "UPDATE Products SET Product_Name = @name WHERE id_Product = @id";
                        using (var cmd = new NpgsqlCommand(updateQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@name", newName);
                            cmd.Parameters.AddWithValue("@id", productId);
                            affected = cmd.ExecuteNonQuery();
                        }
                        break;

                    case "2": // Update Category
                        ViewCategories();
                        Console.Write("Enter new category ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int categoryId)) return;
                        updateQuery = "UPDATE Products SET id_Category = @category WHERE id_Product = @id";
                        using (var cmd = new NpgsqlCommand(updateQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@category", categoryId);
                            cmd.Parameters.AddWithValue("@id", productId);
                            affected = cmd.ExecuteNonQuery();
                        }
                        break;

                    case "3": // Update Price
                        Console.Write("Enter new price: ");
                        if (!decimal.TryParse(Console.ReadLine(), out decimal price)) return;
                        updateQuery = "UPDATE Products SET Price = @price WHERE id_Product = @id";
                        using (var cmd = new NpgsqlCommand(updateQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@price", price);
                            cmd.Parameters.AddWithValue("@id", productId);
                            affected = cmd.ExecuteNonQuery();
                        }
                        break;

                    case "4": // Update Stock
                        Console.Write("Enter new stock quantity: ");
                        if (!int.TryParse(Console.ReadLine(), out int stock)) return;
                        updateQuery = "UPDATE Products SET Stock_Quantity = @stock WHERE id_Product = @id";
                        using (var cmd = new NpgsqlCommand(updateQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@stock", stock);
                            cmd.Parameters.AddWithValue("@id", productId);
                            affected = cmd.ExecuteNonQuery();
                        }
                        break;

                    case "5": // Update Warranty
                        Console.Write("Enter new warranty months: ");
                        if (!int.TryParse(Console.ReadLine(), out int warranty)) return;
                        updateQuery = "UPDATE Products SET Warranty = @warranty WHERE id_Product = @id";
                        using (var cmd = new NpgsqlCommand(updateQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@warranty", warranty);
                            cmd.Parameters.AddWithValue("@id", productId);
                            affected = cmd.ExecuteNonQuery();
                        }
                        break;

                    case "6": // Update Everything
                        ViewCategories();
                        Console.Write("Enter new category ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int newCategoryId)) return;
                        Console.Write("Enter new product name: ");
                        string newProductName = Console.ReadLine() ?? "";
                        Console.Write("Enter new price: ");
                        if (!decimal.TryParse(Console.ReadLine(), out decimal newPrice)) return;
                        Console.Write("Enter new stock quantity: ");
                        if (!int.TryParse(Console.ReadLine(), out int newStock)) return;
                        Console.Write("Enter new warranty months: ");
                        if (!int.TryParse(Console.ReadLine(), out int newWarranty)) newWarranty = 0;

                        updateQuery = @"UPDATE Products SET Product_Name = @name, id_Category = @category,
                            Warranty = @warranty, Price = @price, Stock_Quantity = @stock
                            WHERE id_Product = @id";
                        using (var cmd = new NpgsqlCommand(updateQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@name", newProductName);
                            cmd.Parameters.AddWithValue("@category", newCategoryId);
                            cmd.Parameters.AddWithValue("@warranty", newWarranty);
                            cmd.Parameters.AddWithValue("@price", newPrice);
                            cmd.Parameters.AddWithValue("@stock", newStock);
                            cmd.Parameters.AddWithValue("@id", productId);
                            affected = cmd.ExecuteNonQuery();
                        }
                        break;

                    case "7": // Cancel
                        Console.WriteLine("Update cancelled!");
                        return;

                    default:
                        Console.WriteLine("❌ Invalid option!");
                        return;
                }

                Console.WriteLine(affected > 0 ? "✅ Product updated successfully!" : "❌ No changes made!");
            }
            catch (Exception ex)
            {
                HandleException(ex, "updating product");
            }
        }

        public void RemoveProduct()
        {
            try
            {
                EnsureConnectionOpen();

                ViewAllProducts();
                Console.Write("\nEnter product ID to remove: ");
                if (!int.TryParse(Console.ReadLine(), out int productId)) return;

                // Check for existing orders
                string checkQuery = "SELECT COUNT(*) FROM Ordered_Price WHERE id_Product = @id";
                using var checkCmd = new NpgsqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@id", productId);

                if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                {
                    Console.WriteLine("❌ Cannot remove product with existing orders!");
                    return;
                }

                string deleteQuery = "DELETE FROM Products WHERE id_Product = @id";
                using var deleteCmd = new NpgsqlCommand(deleteQuery, connection);
                deleteCmd.Parameters.AddWithValue("@id", productId);

                int affected = deleteCmd.ExecuteNonQuery();
                Console.WriteLine(affected > 0 ? "✅ Product removed!" : "❌ Product not found!");
            }
            catch (Exception ex)
            {
                HandleException(ex, "removing product");
            }
        }
    }
}