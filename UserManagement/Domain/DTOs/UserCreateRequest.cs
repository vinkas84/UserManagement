using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs
{
    public class UserCreateRequest
    {
        public required string UserName { get; set; }

        public string? FullName { get; set; }

        [EmailAddress]
        public required string Email { get; set; }

        public string? MobileNumber { get; set; }

        public string? Language { get; set; }

        public string? Culture { get; set; }

        public required string Password { get; set; }
    }
}
