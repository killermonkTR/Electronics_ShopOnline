using Electronics_Shop2.Managers;

namespace Electronics_Shop2.UI
{
    public class ConsoleUI
    {
        private int currentStaffId;
        private string currentStaffName;
        private LoginManager loginManager;

        public ConsoleUI(int staffId, string staffName, LoginManager loginManager)
        {
            currentStaffId = staffId;
            currentStaffName = staffName;
            this.loginManager = loginManager;
        }

        public void ShowMainMenu()
        {
            // Test database connection first
            var dbTest = new Data.DatabaseConnection();
            if (!dbTest.TestConnection())
            {
                Console.WriteLine("Cannot start application without database connection!");
                Console.ReadKey();
                return;
            }

            // ✅ Initialize SalesManager with staff info
            var salesManager = new SalesManager(currentStaffId, currentStaffName);
            var clientManager = new ClientManager();
            var staffManager = new StaffManager();
            var productManager = new ProductManager();
            var brandManager = new BrandManager();
            var modelManager = new ModelManager();

            while (true)
            {
                Console.Clear();
                Console.WriteLine($"🏪 ELECTRONICS SHOP MANAGEMENT");
                Console.WriteLine($"👨‍💼 Logged in as: {currentStaffName} (ID: {currentStaffId})");
                Console.WriteLine("=================================");
                Console.WriteLine("1. 👥 Client Management");
                Console.WriteLine("2. 👨‍💼 Staff Management");
                Console.WriteLine("3. 📦 Product Management");
                Console.WriteLine("4. 🏷️ Brand Management");
                Console.WriteLine("5. 📱 Model Management");
                Console.WriteLine("6. 🛒 Sell Products");
                Console.WriteLine("7. 📊 View Reports");
                Console.WriteLine("8. 🔄 Switch Staff");
                Console.WriteLine("9. Exit");
                Console.Write("Choose option: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": clientManager.ShowClientManagement(); break;
                    case "2": staffManager.ShowStaffManagement(); break;
                    case "3": productManager.ShowProductManagement(); break;
                    case "4": brandManager.ShowBrandManagementMenu(); break;
                    case "5": modelManager.ShowModelManagementMenu(); break;
                    case "6": salesManager.SellProducts(); break;
                    case "7": ViewReports(); break;
                    case "8":
                        if (SwitchStaff())
                            return; // Restart the application
                        break;
                    case "9": return;
                    default: Console.WriteLine("Invalid option!"); break;
                }

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }

        private bool SwitchStaff()
        {
            // ✅ Call static method directly
            if (LoginManager.SwitchStaff(out int newStaffId, out string newStaffName))
            {
                currentStaffId = newStaffId;
                currentStaffName = newStaffName;

                Console.WriteLine($"✅ Switched to: {currentStaffName} (ID: {currentStaffId})");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return false; // Don't restart, just update current session
            }
            return false;
        }

        private void ViewReports()
        {
            var reportsManager = new ReportsManager();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("📊 REPORTS DASHBOARD");
                Console.WriteLine("====================");
                Console.WriteLine($"Staff: {currentStaffName}\n");
                Console.WriteLine("1. 📈 Sales Report");
                Console.WriteLine("2. 📦 Stock Report");
                Console.WriteLine("3. 👥 Customer Orders");
                Console.WriteLine("4. 💰 Revenue Analytics");
                Console.WriteLine("5. 🏆 Top Products");
                Console.WriteLine("6. 👨‍💼 Staff Performance");
                Console.WriteLine("7. 🔄 Back to Main Menu");
                Console.Write("Choose option: ");

                var choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1": reportsManager.GenerateSalesReport(); break;
                        case "2": reportsManager.GenerateStockReport(); break;
                        case "3": reportsManager.GenerateCustomerOrdersReport(); break;
                        case "4": reportsManager.GenerateRevenueAnalytics(); break;
                        case "5": reportsManager.GenerateTopProductsReport(); break;
                        case "6": reportsManager.GenerateStaffPerformanceReport(); break;
                        case "7": return;
                        default: Console.WriteLine("Invalid option!"); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error generating report: {ex.Message}");
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }
    }
}