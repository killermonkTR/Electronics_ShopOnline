// Data/OrderRepository.cs
using Electronics_Shop2.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;

namespace Electronics_Shop2.Data
{
    public class OrderRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public OrderRepository()
        {
            _dbConnection = new DatabaseConnection();
        }

        public int CreateOrder(Order order)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var query = @"
                    INSERT INTO Orders (id_Staff, id_Client, id_Payment_Type, Order_Date, Total_Price, Status)
                    VALUES (@StaffId, @ClientId, @PaymentTypeId, @OrderDate, @TotalPrice, @Status)
                    RETURNING id_Order;"; 

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StaffId", order.IdStaff);
                    command.Parameters.AddWithValue("@ClientId", order.IdClient);
                    command.Parameters.AddWithValue("@PaymentTypeId", order.IdPaymentType);
                    command.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                    command.Parameters.AddWithValue("@TotalPrice", order.TotalPrice);
                    command.Parameters.AddWithValue("@Status", order.Status);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public void AddOrderItem(int orderId, int productId, decimal price, int quantity)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var query = @"
                    INSERT INTO Ordered_Price (id_Product, id_Order, Price, Amount)
                    VALUES (@ProductId, @OrderId, @Price, @Amount)";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    command.Parameters.AddWithValue("@Price", price);
                    command.Parameters.AddWithValue("@Amount", quantity);

                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Order> GetAllOrders()
        {
            var orders = new List<Order>();

            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var query = @"
                    SELECT o.*, c.Name as ClientName, s.Staff_Name as StaffName, pt.Payment_Type as PaymentType
                    FROM Orders o
                    JOIN Clients c ON o.id_Client = c.id_Client
                    JOIN Staff s ON o.id_Staff = s.id_Staff
                    JOIN Payment_Types pt ON o.id_Payment_Type = pt.id_Payment_Type
                    ORDER BY o.Order_Date DESC";

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        orders.Add(new Order
                        {
                            IdOrder = reader.GetInt32("id_Order"),
                            IdStaff = reader.GetInt32("id_Staff"),
                            IdClient = reader.GetInt32("id_Client"),
                            IdPaymentType = reader.GetInt32("id_Payment_Type"),
                            OrderDate = reader.GetDateTime("Order_Date"),
                            TotalPrice = reader.GetDecimal("Total_Price"),
                            Status = reader.GetString("Status"),
                            ClientName = reader.GetString("ClientName"),
                            StaffName = reader.GetString("StaffName"),
                            PaymentType = reader.GetString("PaymentType")
                        });
                    }
                }
            }

            return orders;
        }

        public List<OrderItem> GetOrderItems(int orderId)
        {
            var items = new List<OrderItem>();

            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var query = @"
                    SELECT op.*, p.Product_Name
                    FROM Ordered_Price op
                    JOIN Products p ON op.id_Product = p.id_Product
                    WHERE op.id_Order = @OrderId";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new OrderItem
                            {
                                IdProduct = reader.GetInt32("id_Product"),
                                IdOrder = reader.GetInt32("id_Order"),
                                Price = reader.GetDecimal("Price"),
                                Amount = reader.GetInt32("Amount"),
                                ItemTotal = reader.IsDBNull(reader.GetOrdinal("Item_Total")) ? 0 : reader.GetDecimal("Item_Total"), // Safe null handling
                                ProductName = reader.GetString("Product_Name")
                            });
                        }
                    }
                }
            }

            return items;
        }

        // Additional useful methods
        public void UpdateOrderStatus(int orderId, string status)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var query = "UPDATE Orders SET Status = @Status WHERE id_Order = @OrderId";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Status", status);
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public Order GetOrderById(int orderId)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var query = @"
                    SELECT o.*, c.Name as ClientName, s.Staff_Name as StaffName, pt.Payment_Type as PaymentType
                    FROM Orders o
                    JOIN Clients c ON o.id_Client = c.id_Client
                    JOIN Staff s ON o.id_Staff = s.id_Staff
                    JOIN Payment_Types pt ON o.id_Payment_Type = pt.id_Payment_Type
                    WHERE o.id_Order = @OrderId";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Order
                            {
                                IdOrder = reader.GetInt32("id_Order"),
                                IdStaff = reader.GetInt32("id_Staff"),
                                IdClient = reader.GetInt32("id_Client"),
                                IdPaymentType = reader.GetInt32("id_Payment_Type"),
                                OrderDate = reader.GetDateTime("Order_Date"),
                                TotalPrice = reader.GetDecimal("Total_Price"),
                                Status = reader.GetString("Status"),
                                ClientName = reader.GetString("ClientName"),
                                StaffName = reader.GetString("StaffName"),
                                PaymentType = reader.GetString("PaymentType")
                            };
                        }
                    }
                }
            }
            return null;
        }
    }
}