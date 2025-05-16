namespace collegeEventTracker2.Models
{
    public class Users
    {
        public int UserId { get; set; } // Primary key
        public string username { get; set; }
        public string password { get; set; } // Plain password (input ke liye)
        public string email { get; set; }
        public string Role { get; set; } // "Admin" ya "User"
    }
}
