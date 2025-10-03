using Electronics_Shop2.Data;
using Npgsql;

namespace Electronics_Shop2.Managers
{
    public class ReportsManager : BaseManager
    {
        public void GenerateSalesReport()
        {
            try
            {
                EnsureConnectionOpen();
                Console.Clear();
                Console.WriteLine("📈 SALES REPORT");
                Console.WriteLine("====================\n");

                // Total Sales Summary
                string summaryQuery = @"
                    SELECT 
                        COUNT(*) as TotalOrders,
                        SUM(Total_Price) as TotalRevenue,
                        AVG(Total_Price) as AverageOrderValue,
                        MIN(Order_Date) as FirstOrderDate,
                        MAX(Order_Date) as LastOrderDate
                    FROM Orders";

                using var summaryCmd = new NpgsqlCommand(summaryQuery, connection);
                using var summaryReader = summaryCmd.ExecuteReader();

                if (summaryReader.Read())
                {
                    Console.WriteLine("📊 SALES SUMMARY:");
                    Console.WriteLine($"   Total Orders: {GetSafeInt(summaryReader["TotalOrders"])}");
                    Console.WriteLine($"   Total Revenue: ${GetSafeDecimal(summaryReader["TotalRevenue"]):F2}");
                    Console.WriteLine($"   Average Order Value: ${GetSafeDecimal(summaryReader["AverageOrderValue"]):F2}");
                    Console.WriteLine($"   Date Range: {GetSafeString(summaryReader["FirstOrderDate"]):d} to {GetSafeString(summaryReader["LastOrderDate"]):d}");
                }
                summaryReader.Close();

                Console.WriteLine("\n📅 RECENT ORDERS:");
                string ordersQuery = @"
                    SELECT o.id_Order, c.Name as Customer, s.Staff_Name as Staff, 
                           o.Order_Date, o.Total_Price, o.Status,
                           COUNT(op.id_Product) as ItemsCount
                    FROM Orders o
                    JOIN Clients c ON o.id_Client = c.id_Client
                    JOIN Staff s ON o.id_Staff = s.id_Staff
                    LEFT JOIN Ordered_Price op ON o.id_Order = op.id_Order
                    GROUP BY o.id_Order, c.Name, s.Staff_Name, o.Order_Date, o.Total_Price, o.Status
                    ORDER BY o.Order_Date DESC
                    LIMIT 10";

                using var ordersCmd = new NpgsqlCommand(ordersQuery, connection);
                using var ordersReader = ordersCmd.ExecuteReader();

                while (ordersReader.Read())
                {
                    Console.WriteLine($"   Order #{GetSafeInt(ordersReader["id_Order"])} | {GetSafeString(ordersReader["Customer"])}");
                    Console.WriteLine($"   Date: {GetSafeString(ordersReader["Order_Date"]):d} | Total: ${GetSafeDecimal(ordersReader["Total_Price"]):F2}");
                    Console.WriteLine($"   Items: {GetSafeInt(ordersReader["ItemsCount"])} | Status: {GetSafeString(ordersReader["Status"])}");
                    Console.WriteLine($"   Processed by: {GetSafeString(ordersReader["Staff"])}");
                    Console.WriteLine("   ─────────────────────────────────");
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "generating sales report");
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        public void GenerateStockReport()
        {
            try
            {
                EnsureConnectionOpen();
                Console.Clear();
                Console.WriteLine("📦 STOCK REPORT");
                Console.WriteLine("====================\n");

                // Stock Summary
                string summaryQuery = @"
                    SELECT 
                        COUNT(*) as TotalProducts,
                        SUM(Stock_Quantity) as TotalStock,
                        SUM(CASE WHEN Stock_Quantity = 0 THEN 1 ELSE 0 END) as OutOfStock,
                        SUM(CASE WHEN Stock_Quantity <= 2 THEN 1 ELSE 0 END) as LowStock,
                        AVG(Price) as AveragePrice
                    FROM Products";

                using var summaryCmd = new NpgsqlCommand(summaryQuery, connection);
                using var summaryReader = summaryCmd.ExecuteReader();

                if (summaryReader.Read())
                {
                    Console.WriteLine("📊 STOCK SUMMARY:");
                    Console.WriteLine($"   Total Products: {GetSafeInt(summaryReader["TotalProducts"])}");
                    Console.WriteLine($"   Total Items in Stock: {GetSafeInt(summaryReader["TotalStock"])}");
                    Console.WriteLine($"   Out of Stock: {GetSafeInt(summaryReader["OutOfStock"])}");
                    Console.WriteLine($"   Low Stock (≤2): {GetSafeInt(summaryReader["LowStock"])}");
                    Console.WriteLine($"   Average Price: ${GetSafeDecimal(summaryReader["AveragePrice"]):F2}");
                }
                summaryReader.Close();

                Console.WriteLine("\n🔴 LOW STOCK ITEMS:");
                string lowStockQuery = @"
                    SELECT p.id_Product, p.Product_Name, b.Brand_Name, 
                           p.Stock_Quantity, p.Price,
                           CASE 
                               WHEN p.Stock_Quantity = 0 THEN '⛔ OUT OF STOCK'
                               WHEN p.Stock_Quantity <= 2 THEN '🔴 VERY LOW'
                               WHEN p.Stock_Quantity <= 5 THEN '🟡 LOW'
                               ELSE '🟢 GOOD'
                           END as StockStatus
                    FROM Products p
                    JOIN Phone_Models pm ON p.id_Model = pm.id_Model
                    JOIN Brands b ON pm.id_Brand = b.id_Brand
                    WHERE p.Stock_Quantity <= 5
                    ORDER BY p.Stock_Quantity ASC, p.Product_Name";

                using var lowStockCmd = new NpgsqlCommand(lowStockQuery, connection);
                using var lowStockReader = lowStockCmd.ExecuteReader();

                bool hasLowStock = false;
                while (lowStockReader.Read())
                {
                    hasLowStock = true;
                    Console.WriteLine($"   {GetSafeString(lowStockReader["Product_Name"])}");
                    Console.WriteLine($"     Brand: {GetSafeString(lowStockReader["Brand_Name"])}");
                    Console.WriteLine($"     Stock: {GetSafeInt(lowStockReader["Stock_Quantity"])} | Status: {GetSafeString(lowStockReader["StockStatus"])}");
                    Console.WriteLine($"     Price: ${GetSafeDecimal(lowStockReader["Price"]):F2}");
                    Console.WriteLine("     ─────────────────────────────────");
                }

                if (!hasLowStock)
                {
                    Console.WriteLine("   🎉 All products have sufficient stock!");
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "generating stock report");
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        public void GenerateCustomerOrdersReport()
        {
            try
            {
                EnsureConnectionOpen();
                Console.Clear();
                Console.WriteLine("👥 CUSTOMER ORDERS REPORT");
                Console.WriteLine("====================\n");

                string query = @"
                    SELECT 
                        c.id_Client,
                        c.Name as CustomerName,
                        c.Email,
                        c.Registration_Date,
                        COUNT(o.id_Order) as TotalOrders,
                        SUM(o.Total_Price) as TotalSpent,
                        AVG(o.Total_Price) as AverageOrderValue,
                        MAX(o.Order_Date) as LastOrderDate
                    FROM Clients c
                    LEFT JOIN Orders o ON c.id_Client = o.id_Client
                    GROUP BY c.id_Client, c.Name, c.Email, c.Registration_Date
                    ORDER BY TotalSpent DESC, TotalOrders DESC";

                using var command = new NpgsqlCommand(query, connection);
                using var reader = command.ExecuteReader();

                Console.WriteLine("🏆 TOP CUSTOMERS BY SPENDING:");
                int rank = 1;
                while (reader.Read())
                {
                    string customerName = GetSafeString(reader["CustomerName"]);
                    int totalOrders = GetSafeInt(reader["TotalOrders"]);
                    decimal totalSpent = GetSafeDecimal(reader["TotalSpent"]);

                    Console.WriteLine($"   {rank}. {customerName}");
                    Console.WriteLine($"      Orders: {totalOrders} | Total Spent: ${totalSpent:F2}");
                    Console.WriteLine($"      Avg Order: ${GetSafeDecimal(reader["AverageOrderValue"]):F2}");
                    Console.WriteLine($"      Last Order: {GetSafeString(reader["LastOrderDate"]):d}");
                    Console.WriteLine($"      Registered: {GetSafeString(reader["Registration_Date"]):d}");
                    Console.WriteLine("      ─────────────────────────────────");
                    rank++;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "generating customer orders report");
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        public void GenerateRevenueAnalytics()
        {
            try
            {
                EnsureConnectionOpen();
                Console.Clear();
                Console.WriteLine("💰 REVENUE ANALYTICS");
                Console.WriteLine("====================\n");

                // Monthly Revenue
                Console.WriteLine("📅 MONTHLY REVENUE:");
                string monthlyQuery = @"
                    SELECT 
                        DATE_FORMAT(Order_Date, '%Y-%m') as Month,
                        COUNT(*) as OrderCount,
                        SUM(Total_Price) as MonthlyRevenue,
                        AVG(Total_Price) as AvgOrderValue
                    FROM Orders
                    GROUP BY DATE_FORMAT(Order_Date, '%Y-%m')
                    ORDER BY Month DESC
                    LIMIT 6";

                using var monthlyCmd = new NpgsqlCommand(monthlyQuery, connection);
                using var monthlyReader = monthlyCmd.ExecuteReader();

                while (monthlyReader.Read())
                {
                    Console.WriteLine($"   {GetSafeString(monthlyReader["Month"])}");
                    Console.WriteLine($"     Orders: {GetSafeInt(monthlyReader["OrderCount"])}");
                    Console.WriteLine($"     Revenue: ${GetSafeDecimal(monthlyReader["MonthlyRevenue"]):F2}");
                    Console.WriteLine($"     Avg Order: ${GetSafeDecimal(monthlyReader["AvgOrderValue"]):F2}");
                    Console.WriteLine("     ─────────────────────────────────");
                }
                monthlyReader.Close();

                // Revenue by Payment Type
                Console.WriteLine("\n💳 REVENUE BY PAYMENT METHOD:");
                string paymentQuery = @"
                    SELECT 
                        pt.Payment_Type,
                        COUNT(o.id_Order) as OrderCount,
                        SUM(o.Total_Price) as TotalRevenue
                    FROM Orders o
                    JOIN Payment_Types pt ON o.id_Payment_Type = pt.id_Payment_Type
                    GROUP BY pt.Payment_Type
                    ORDER BY TotalRevenue DESC";

                using var paymentCmd = new NpgsqlCommand(paymentQuery, connection);
                using var paymentReader = paymentCmd.ExecuteReader();

                while (paymentReader.Read())
                {
                    Console.WriteLine($"   {GetSafeString(paymentReader["Payment_Type"])}");
                    Console.WriteLine($"     Orders: {GetSafeInt(paymentReader["OrderCount"])}");
                    Console.WriteLine($"     Revenue: ${GetSafeDecimal(paymentReader["TotalRevenue"]):F2}");
                    Console.WriteLine("     ─────────────────────────────────");
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "generating revenue analytics");
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        public void GenerateTopProductsReport()
        {
            try
            {
                EnsureConnectionOpen();
                Console.Clear();
                Console.WriteLine("🏆 TOP PRODUCTS REPORT");
                Console.WriteLine("====================\n");

                string query = @"
                    SELECT 
                        p.id_Product,
                        p.Product_Name,
                        b.Brand_Name,
                        c.Category_Name,
                        p.Price,
                        p.Stock_Quantity,
                        SUM(op.Amount) as TotalSold,
                        SUM(op.Item_Total) as TotalRevenue
                    FROM Products p
                    JOIN Phone_Models pm ON p.id_Model = pm.id_Model
                    JOIN Brands b ON pm.id_Brand = b.id_Brand
                    JOIN Categories c ON p.id_Category = c.id_Category
                    LEFT JOIN Ordered_Price op ON p.id_Product = op.id_Product
                    GROUP BY p.id_Product, p.Product_Name, b.Brand_Name, c.Category_Name, p.Price, p.Stock_Quantity
                    ORDER BY TotalRevenue DESC, TotalSold DESC
                    LIMIT 10";

                using var command = new NpgsqlCommand(query, connection);
                using var reader = command.ExecuteReader();

                Console.WriteLine("🔥 BEST SELLING PRODUCTS:");
                int rank = 1;
                while (reader.Read())
                {
                    string productName = GetSafeString(reader["Product_Name"]);
                    int totalSold = GetSafeInt(reader["TotalSold"]);
                    decimal totalRevenue = GetSafeDecimal(reader["TotalRevenue"]);

                    Console.WriteLine($"   {rank}. {productName}");
                    Console.WriteLine($"      Brand: {GetSafeString(reader["Brand_Name"])} | Category: {GetSafeString(reader["Category_Name"])}");
                    Console.WriteLine($"      Sold: {totalSold} units | Revenue: ${totalRevenue:F2}");
                    Console.WriteLine($"      Price: ${GetSafeDecimal(reader["Price"]):F2} | Stock: {GetSafeInt(reader["Stock_Quantity"])}");
                    Console.WriteLine("      ─────────────────────────────────");
                    rank++;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "generating top products report");
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        public void GenerateStaffPerformanceReport()
        {
            try
            {
                EnsureConnectionOpen();
                Console.Clear();
                Console.WriteLine("👨‍💼 STAFF PERFORMANCE REPORT");
                Console.WriteLine("====================\n");

                string query = @"
                    SELECT 
                        s.id_Staff,
                        s.Staff_Name,
                        p.Position,
                        s.Hire_Date,
                        COUNT(o.id_Order) as OrdersProcessed,
                        SUM(o.Total_Price) as TotalRevenue,
                        AVG(o.Total_Price) as AvgOrderValue,
                        MAX(o.Order_Date) as LastOrderDate
                    FROM Staff s
                    JOIN Positions p ON s.id_Position = p.id_Position
                    LEFT JOIN Orders o ON s.id_Staff = o.id_Staff
                    GROUP BY s.id_Staff, s.Staff_Name, p.Position, s.Hire_Date
                    ORDER BY TotalRevenue DESC, OrdersProcessed DESC";

                using var command = new NpgsqlCommand(query, connection);
                using var reader = command.ExecuteReader();

                Console.WriteLine("⭐ STAFF PERFORMANCE RANKING:");
                int rank = 1;
                while (reader.Read())
                {
                    string staffName = GetSafeString(reader["Staff_Name"]);
                    int ordersProcessed = GetSafeInt(reader["OrdersProcessed"]);
                    decimal totalRevenue = GetSafeDecimal(reader["TotalRevenue"]);

                    Console.WriteLine($"   {rank}. {staffName} ({GetSafeString(reader["Position"])})");
                    Console.WriteLine($"      Orders Processed: {ordersProcessed}");
                    Console.WriteLine($"      Total Revenue: ${totalRevenue:F2}");
                    Console.WriteLine($"      Avg Order Value: ${GetSafeDecimal(reader["AvgOrderValue"]):F2}");
                    Console.WriteLine($"      Last Order: {GetSafeString(reader["LastOrderDate"]):d}");
                    Console.WriteLine($"      Hired: {GetSafeString(reader["Hire_Date"]):d}");
                    Console.WriteLine("      ─────────────────────────────────");
                    rank++;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "generating staff performance report");
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }
    }
}