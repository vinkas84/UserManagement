using Domain.Models;

namespace DataAccess.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> CreateUserAsync(User user);
        Task<User?> GetUserByIdAsync(int id);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
    }
}
