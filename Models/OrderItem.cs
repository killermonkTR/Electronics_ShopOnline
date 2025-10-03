

namespace Electronics_Shop2.Models
{
    public class OrderItem
    {
        public int IdProduct { get; set; }
        public int IdOrder { get; set; }
        public decimal Price { get; set; }
        public int Amount { get; set; }
        public decimal ItemTotal { get; set; }
        public string ProductName { get; set; } = string.Empty;
    }
}
