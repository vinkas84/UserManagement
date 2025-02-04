namespace Domain.Models
{
    public class User
    {
        public int Id { get; set; }

        public required string UserName { get; set; }

        public string? FullName { get; set; }

        public required string Email { get; set; }

        public string? MobileNumber { get; set; }

        public string? Language { get; set; }

        public string? Culture { get; set; }

        public required string PasswordHash { get; set; } // Store hashed password
    }
}
