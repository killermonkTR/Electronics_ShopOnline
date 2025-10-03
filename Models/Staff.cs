 

namespace Electronics_Shop2.Models
{
    public class Staff
    {
        public int IdStaff { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public int IdPosition { get; set; }
        public string? PhoneNumber { get; set; }
        public decimal? Salary { get; set; }
        public DateTime HireDate { get; set; }
        public string Position { get; set; } = string.Empty;
    }
}
