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
                Console.WriteLine($"   Category: {GetSafeString(reader["Category_Name"])} | Brand: {GetSafeString(reader["Brand_Name"])}");
                Console.WriteLine($"   Price: ${GetSafeDecimal(reader["Price"])} | Stock: {GetSafeInt(reader["Stock_Quantity"])}");
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

                // ✅ FIXED: No parameters needed - ModelManager uses BaseManager
                var modelManager = new ModelManager();
                int modelId = modelManager.AddNewModel();
                if (modelId == -1) return -1;

                ViewCategories();
                Console.Write("Enter category ID: ");
                if (!int.TryParse(Console.ReadLine(), out int categoryId)) return -1;

                Console.Write("Enter product name: ");
                string name = Console.ReadLine() ?? "";
                Console.Write("Enter price: ");
                if (!decimal.TryParse(Console.ReadLine(), out decimal price)) return -1;
                Console.Write("Enter stock quantity: ");
                if (!int.TryParse(Console.ReadLine(), out int stock)) return -1;
                Console.Write("Enter warranty months: ");
                if (!int.TryParse(Console.ReadLine(), out int warranty)) warranty = 0;

                string query = @"INSERT INTO Products (Product_Name, id_Category, id_Model, Warranty, Price, Stock_Quantity) 
                            VALUES (@name, @category, @model, @warranty, @price, @stock);
                            SELECT LAST_INSERT_ID();";

                using var command = new NpgsqlCommand(query, connection);
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

                ViewCategories();
                Console.Write("Enter new category ID: ");
                if (!int.TryParse(Console.ReadLine(), out int categoryId)) return;

                Console.Write("Enter new product name: ");
                string name = Console.ReadLine() ?? "";
                Console.Write("Enter new price: ");
                if (!decimal.TryParse(Console.ReadLine(), out decimal price)) return;
                Console.Write("Enter new stock quantity: ");
                if (!int.TryParse(Console.ReadLine(), out int stock)) return;
                Console.Write("Enter new warranty months: ");
                if (!int.TryParse(Console.ReadLine(), out int warranty)) warranty = 0;

                string query = @"UPDATE Products SET Product_Name = @name, id_Category = @category,
                            Warranty = @warranty, Price = @price, Stock_Quantity = @stock
                            WHERE id_Product = @id";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@category", categoryId);
                command.Parameters.AddWithValue("@warranty", warranty);
                command.Parameters.AddWithValue("@price", price);
                command.Parameters.AddWithValue("@stock", stock);
                command.Parameters.AddWithValue("@id", productId);

                int affected = command.ExecuteNonQuery();
                Console.WriteLine(affected > 0 ? "✅ Product updated!" : "❌ Product not found!");
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