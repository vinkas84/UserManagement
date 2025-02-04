using Domain.DTOs;
using Domain.Models;

namespace Business.Interfaces
{
    public interface IUserManager
    {
        Task<User?> CreateUserAsync(UserCreateRequest request);
        Task<User?> GetUserAsync(int id);
        Task<bool> UpdateUserAsync(int id, UserUpdateRequest request);
        Task<bool> DeleteUserAsync(int id);
        bool ValidatePassword(User user, string password); 
        Task<bool> UpdatePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}
