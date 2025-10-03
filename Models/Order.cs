

namespace Electronics_Shop2.Models
{
    public class Order
    {
        public int IdOrder { get; set; }
        public int IdStaff { get; set; }
        public int IdClient { get; set; }
        public int IdPaymentType { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Pending";
        public string ClientName { get; set; } = string.Empty;
        public string StaffName { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
    }
}
