

namespace Electronics_Shop2.Models
{
    public class Product
    {
        public int IdProduct { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int IdCategory { get; set; }
        public int IdModel { get; set; }
        public int? Warranty { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
    }
}
