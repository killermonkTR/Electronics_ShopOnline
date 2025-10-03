using Electronics_Shop2.UI;
using Electronics_Shop2.Managers;

namespace Electronics_Shop2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            // ✅ Call static method directly - no instance needed
            var loginResult = LoginManager.StaffLogin();
            if (!loginResult.success)
            {
                Console.WriteLine("Login failed. Exiting program...");
                Console.ReadKey();
                return;
            }

            // ✅ Create LoginManager instance to pass to ConsoleUI
            var loginManager = new LoginManager();

            // Start main application with logged-in staff
            var ui = new ConsoleUI(loginResult.staffId, loginResult.staffName, loginManager);
            ui.ShowMainMenu();
        }
    }
}