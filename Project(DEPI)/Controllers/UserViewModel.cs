namespace Project_DEPI_.Controllers
{
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int BookingsCount { get; set; }
        public string RoleNames { get; set; } = string.Empty;
        public List<string>? Roles { get; set; }  
    }
}