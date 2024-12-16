namespace CommunityHub.API.Models
{
    public class RegisterRequest
    {
        public string? Name { get; set; }  // User's name
        public string? Email { get; set; } // User's email address
        public string? Password { get; set; } // User's password (plain text)
    }
}
