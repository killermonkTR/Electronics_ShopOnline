using Electronics_Shop2.Data;
using Npgsql;

namespace Electronics_Shop2.Managers
{
    public class SalesManager : BaseManager
    {
        private int currentStaffId;
        private string currentStaffName;

        public SalesManager(int staffId, string staffName)
        {
            currentStaffId = staffId;
            currentStaffName = staffName;
        }

        public void SellProducts()
        {
            try
            {
                EnsureConnectionOpen();
                Console.Clear();
                Console.WriteLine("🛒 SELL MULTIPLE PRODUCTS");
                Console.WriteLine($"👨‍💼 Staff: {currentStaffName} (ID: {currentStaffId})");
                Console.WriteLine("=================================\n");

                // Select client
                var clientManager = new ClientManager();
                clientManager.UseExistingConnection(connection);
                clientManager.ViewAllClients();
                Console.Write("\nEnter client ID: ");
                if (!int.TryParse(Console.ReadLine(), out int clientId)) return;

                // Verify client exists
                string clientQuery = "SELECT Name FROM Clients WHERE id_Client = @id";
                using var clientCmd = new NpgsqlCommand(clientQuery, connection);
                clientCmd.Parameters.AddWithValue("@id", clientId);
                var clientName = clientCmd.ExecuteScalar()?.ToString();
                if (clientName == null)
                {
                    Console.WriteLine("❌ Client not found!");
                    return;
                }

                // Create order first - set a default payment type or allow NULL
                string orderQuery = @"INSERT INTO Orders (id_Staff, id_Client, id_Payment_Type, Order_Date, Status, Total_Price) 
                VALUES (@staff, @client, 1, @orderDate, 'Processing', 0.00)
                RETURNING id_Order;";

                int orderId;
                using (var orderCmd = new NpgsqlCommand(orderQuery, connection))
                {
                    orderCmd.Parameters.AddWithValue("@staff", currentStaffId);
                    orderCmd.Parameters.AddWithValue("@client", clientId);
                    orderCmd.Parameters.AddWithValue("@orderDate", DateTime.Today);
                    orderId = Convert.ToInt32(orderCmd.ExecuteScalar());
                }

                Console.WriteLine($"\n✅ Order #{orderId} created for {clientName}");
                Console.WriteLine($"   Processed by: {currentStaffName}");
                Console.WriteLine("Now add products to this order...\n");

                // Add multiple products to order
                var cart = new List<CartItem>();
                bool addingProducts = true;

                while (addingProducts)
                {
                    Console.Clear();
                    Console.WriteLine($"🛒 ADDING PRODUCTS TO ORDER #{orderId}");
                    Console.WriteLine($"Client: {clientName} | Staff: {currentStaffName}");
                    Console.WriteLine("=================================\n");

                    // Show current cart
                    if (cart.Count > 0)
                    {
                        Console.WriteLine("Current Cart:");
                        decimal cartTotal = 0;
                        for (int i = 0; i < cart.Count; i++)
                        {
                            var item = cart[i];
                            decimal itemTotal = item.Price * item.Quantity;
                            cartTotal += itemTotal;
                            Console.WriteLine($"  {i + 1}. {item.ProductName} x {item.Quantity} = ${itemTotal:F2}");
                        }
                        Console.WriteLine($"  Total: ${cartTotal:F2}\n");
                    }

                    // Show available products
                    var productManager = new ProductManager();
                    productManager.UseExistingConnection(connection);
                    productManager.ViewAllProducts();

                    Console.WriteLine("\nOptions:");
                    Console.WriteLine("  [Product ID] - Add product to cart");
                    Console.WriteLine("  [0] - Finish and proceed to payment");
                    Console.WriteLine("  [999] - Cancel order");
                    Console.Write("\nEnter Product ID or option: ");

                    if (!int.TryParse(Console.ReadLine(), out int input)) continue;

                    if (input == 0)
                    {
                        addingProducts = false;
                    }
                    else if (input == 999)
                    {
                        // Cancel order
                        string cancelQuery = "DELETE FROM Orders WHERE id_Order = @id";
                        using var cancelCmd = new NpgsqlCommand(cancelQuery, connection);
                        cancelCmd.Parameters.AddWithValue("@id", orderId);
                        cancelCmd.ExecuteNonQuery();
                        Console.WriteLine("❌ Order cancelled!");
                        return;
                    }
                    else
                    {
                        // Add product to cart
                        AddProductToCart(input, cart);
                    }
                }

                // Process the cart and ask for payment type at the end
                if (cart.Count > 0)
                {
                    ProcessCartWithPayment(orderId, cart, clientName);
                }
                else
                {
                    // Delete empty order
                    string deleteQuery = "DELETE FROM Orders WHERE id_Order = @id";
                    using var deleteCmd = new NpgsqlCommand(deleteQuery, connection);
                    deleteCmd.Parameters.AddWithValue("@id", orderId);
                    deleteCmd.ExecuteNonQuery();
                    Console.WriteLine("❌ No products added. Order cancelled!");
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "selling products");
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        private void AddProductToCart(int productId, List<CartItem> cart)
        {
            try
            {
                // Check product stock and price
                string productQuery = "SELECT Product_Name, Stock_Quantity, Price FROM Products WHERE id_Product = @id";
                using var productCmd = new NpgsqlCommand(productQuery, connection);
                productCmd.Parameters.AddWithValue("@id", productId);
                using var productReader = productCmd.ExecuteReader();

                if (!productReader.Read())
                {
                    Console.WriteLine("❌ Product not found!");
                    return;
                }

                string productName = GetSafeString(productReader["Product_Name"]);
                int stock = GetSafeInt(productReader["Stock_Quantity"]);
                decimal price = GetSafeDecimal(productReader["Price"]);
                productReader.Close();

                if (stock <= 0)
                {
                    Console.WriteLine($"❌ Product '{productName}' is out of stock!");
                    return;
                }

                Console.Write($"Enter quantity for '{productName}' (Available: {stock}): ");
                if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
                {
                    Console.WriteLine("❌ Invalid quantity!");
                    return;
                }

                if (quantity > stock)
                {
                    Console.WriteLine($"❌ Not enough stock! Only {stock} available.");
                    return;
                }

                // Check if product already in cart
                var existingItem = cart.FirstOrDefault(item => item.ProductId == productId);
                if (existingItem != null)
                {
                    if (existingItem.Quantity + quantity > stock)
                    {
                        Console.WriteLine($"❌ Total quantity would exceed available stock!");
                        return;
                    }
                    existingItem.Quantity += quantity;
                    Console.WriteLine($"✅ Updated quantity: {existingItem.ProductName} x {existingItem.Quantity}");
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ProductId = productId,
                        ProductName = productName,
                        Price = price,
                        Quantity = quantity
                    });
                    Console.WriteLine($"✅ Added to cart: {productName} x {quantity}");
                }

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                HandleException(ex, "adding product to cart");
            }
        }

        private void ProcessCartWithPayment(int orderId, List<CartItem> cart, string clientName)
        {
            try
            {
                decimal orderTotal = 0;

                // Calculate total first
                foreach (var item in cart)
                {
                    orderTotal += item.Price * item.Quantity;
                }

                // Show order summary before payment
                Console.Clear();
                Console.WriteLine("💰 ORDER SUMMARY");
                Console.WriteLine("================\n");
                Console.WriteLine($"Order #: {orderId}");
                Console.WriteLine($"Client: {clientName}");
                Console.WriteLine($"Staff: {currentStaffName}");
                Console.WriteLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm}\n");

                Console.WriteLine("Products:");
                foreach (var item in cart)
                {
                    decimal itemTotal = item.Price * item.Quantity;
                    Console.WriteLine($"  {item.ProductName} x {item.Quantity} @ ${item.Price:F2} = ${itemTotal:F2}");
                }

                Console.WriteLine($"\n📊 ORDER TOTAL: ${orderTotal:F2}");
                Console.WriteLine("\n" + new string('=', 40));

                // Ask for payment type at the end
                Console.WriteLine("\n💳 SELECT PAYMENT METHOD:");
                ViewPaymentTypes();
                Console.Write("Enter payment type ID: ");
                if (!int.TryParse(Console.ReadLine(), out int paymentTypeId))
                {
                    Console.WriteLine("❌ Invalid payment type!");
                    return;
                }

                // Verify payment type exists
                string paymentQuery = "SELECT Payment_Type FROM Payment_Types WHERE id_Payment_Type = @id";
                using var paymentCmd = new NpgsqlCommand(paymentQuery, connection);
                paymentCmd.Parameters.AddWithValue("@id", paymentTypeId);
                var paymentType = paymentCmd.ExecuteScalar()?.ToString();
                if (paymentType == null)
                {
                    Console.WriteLine("❌ Payment type not found!");
                    return;
                }

                // Add products to Ordered_Price and update stock
                foreach (var item in cart)
                {
                    string insertQuery = @"INSERT INTO Ordered_Price (id_Product, id_Order, Price, Amount) 
                                    VALUES (@product, @order, @price, @amount)";

                    using var insertCmd = new NpgsqlCommand(insertQuery, connection);
                    insertCmd.Parameters.AddWithValue("@product", item.ProductId);
                    insertCmd.Parameters.AddWithValue("@order", orderId);
                    insertCmd.Parameters.AddWithValue("@price", item.Price);
                    insertCmd.Parameters.AddWithValue("@amount", item.Quantity);
                    insertCmd.ExecuteNonQuery();

                    // Update stock
                    string updateStockQuery = "UPDATE Products SET Stock_Quantity = Stock_Quantity - @quantity WHERE id_Product = @id";
                    using var stockCmd = new NpgsqlCommand(updateStockQuery, connection);
                    stockCmd.Parameters.AddWithValue("@quantity", item.Quantity);
                    stockCmd.Parameters.AddWithValue("@id", item.ProductId);
                    stockCmd.ExecuteNonQuery();
                }

                // Update order with payment type and total
                string updateOrderQuery = @"UPDATE Orders SET Total_Price = @total, 
                                          id_Payment_Type = @payment, Status = 'Completed' 
                                          WHERE id_Order = @id";
                using var orderCmd = new NpgsqlCommand(updateOrderQuery, connection);
                orderCmd.Parameters.AddWithValue("@total", orderTotal);
                orderCmd.Parameters.AddWithValue("@payment", paymentTypeId);
                orderCmd.Parameters.AddWithValue("@id", orderId);
                orderCmd.ExecuteNonQuery();

                // Display final receipt
                Console.Clear();
                Console.WriteLine("🎉 ORDER COMPLETED!");
                Console.WriteLine("====================\n");
                Console.WriteLine($"Order #: {orderId}");
                Console.WriteLine($"Client: {clientName}");
                Console.WriteLine($"Staff: {currentStaffName}");
                Console.WriteLine($"Payment: {paymentType}");
                Console.WriteLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm}\n");

                Console.WriteLine("Products:");
                foreach (var item in cart)
                {
                    decimal itemTotal = item.Price * item.Quantity;
                    Console.WriteLine($"  {item.ProductName} x {item.Quantity} @ ${item.Price:F2} = ${itemTotal:F2}");
                }

                Console.WriteLine($"\n💵 TOTAL: ${orderTotal:F2}");
                Console.WriteLine("\nThank you for your business! 🛍️");
            }
            catch (Exception ex)
            {
                HandleException(ex, "processing cart with payment");
            }
        }

        public void ViewPaymentTypes()
        {
            try
            {
                EnsureConnectionOpen();

                Console.WriteLine("\nAvailable Payment Methods:");
                string query = "SELECT id_Payment_Type, Payment_Type FROM Payment_Types";
                using var command = new NpgsqlCommand(query, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Console.WriteLine($"  ID: {reader["id_Payment_Type"]} | {GetSafeString(reader["Payment_Type"])}");
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "viewing payment types");
            }
        }
    }

    // Cart item class
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}