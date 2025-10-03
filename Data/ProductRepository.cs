// Data/ProductRepository.cs
using Electronics_Shop2.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;

namespace Electronics_Shop2.Data
{
    public class ProductRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public ProductRepository()
        {
            _dbConnection = new DatabaseConnection();
        }

        public List<Product> GetAllProducts()
        {
            var products = new List<Product>();

            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var query = @"
                    SELECT p.*, c.Category_Name, b.Brand_Name, pm.Phone_Model as Model_Name
                    FROM Products p
                    JOIN Categories c ON p.id_Category = c.id_Category
                    JOIN Phone_Models pm ON p.id_Model = pm.id_Model
                    JOIN Brands b ON pm.id_Brand = b.id_Brand";

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            IdProduct = reader.GetInt32("id_Product"),
                            ProductName = reader.GetString("Product_Name"),
                            IdCategory = reader.GetInt32("id_Category"),
                            IdModel = reader.GetInt32("id_Model"),
                            Warranty = reader.IsDBNull("Warranty") ? null : reader.GetInt32("Warranty"),
                            Price = reader.GetDecimal("Price"),
                            StockQuantity = reader.GetInt32("Stock_Quantity"),
                            CategoryName = reader.GetString("Category_Name"),
                            BrandName = reader.GetString("Brand_Name"),
                            ModelName = reader.GetString("Model_Name")
                        });
                    }
                }
            }

            return products;
        }

        public void AddProduct(Product product)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var query = @"
                    INSERT INTO Products (Product_Name, id_Category, id_Model, Warranty, Price, Stock_Quantity)
                    VALUES (@ProductName, @CategoryId, @ModelId, @Warranty, @Price, @StockQuantity)";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductName", product.ProductName);
                    command.Parameters.AddWithValue("@CategoryId", product.IdCategory);
                    command.Parameters.AddWithValue("@ModelId", product.IdModel);
                    command.Parameters.AddWithValue("@Warranty", product.Warranty ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);

                    command.ExecuteNonQuery();
                }
            }
        }

        public Product? GetProductById(int productId)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var query = @"
                    SELECT p.*, c.Category_Name, b.Brand_Name, pm.Phone_Model as Model_Name
                    FROM Products p
                    JOIN Categories c ON p.id_Category = c.id_Category
                    JOIN Phone_Models pm ON p.id_Model = pm.id_Model
                    JOIN Brands b ON pm.id_Brand = b.id_Brand
                    WHERE p.id_Product = @ProductId";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Product
                            {
                                IdProduct = reader.GetInt32("id_Product"),
                                ProductName = reader.GetString("Product_Name"),
                                IdCategory = reader.GetInt32("id_Category"),
                                IdModel = reader.GetInt32("id_Model"),
                                Warranty = reader.IsDBNull("Warranty") ? null : reader.GetInt32("Warranty"),
                                Price = reader.GetDecimal("Price"),
                                StockQuantity = reader.GetInt32("Stock_Quantity"),
                                CategoryName = reader.GetString("Category_Name"),
                                BrandName = reader.GetString("Brand_Name"),
                                ModelName = reader.GetString("Model_Name")
                            };
                        }
                    }
                }
            }

            return null;
        }

        public void UpdateProduct(Product product)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var query = @"
                    UPDATE Products 
                    SET Product_Name = @ProductName, 
                        id_Category = @CategoryId, 
                        id_Model = @ModelId, 
                        Warranty = @Warranty, 
                        Price = @Price, 
                        Stock_Quantity = @StockQuantity
                    WHERE id_Product = @ProductId";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", product.IdProduct);
                    command.Parameters.AddWithValue("@ProductName", product.ProductName);
                    command.Parameters.AddWithValue("@CategoryId", product.IdCategory);
                    command.Parameters.AddWithValue("@ModelId", product.IdModel);
                    command.Parameters.AddWithValue("@Warranty", product.Warranty ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteProduct(int productId)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var query = "DELETE FROM Products WHERE id_Product = @ProductId";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}