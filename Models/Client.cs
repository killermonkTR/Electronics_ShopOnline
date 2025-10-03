 

namespace Electronics_Shop2.Models
{
    public class Client
    {
        public int IdClient { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}
