using System.Text;
using System.Security.Cryptography;
using Domain.Models;
using Business.Interfaces;
using DataAccess.Interfaces;
using Domain.DTOs;

namespace Infrastructure
{

    public class UserManager : IUserManager
    {
        private readonly IUserRepository _userRepository;

        public UserManager(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> CreateUserAsync(UserCreateRequest request)
        {
            var passwordHash = HashPassword(request.Password);

            var user = new User
            {
                UserName = request.UserName,
                FullName = request.FullName,
                Email = request.Email,
                MobileNumber = request.MobileNumber,
                Language = request.Language,
                Culture = request.Culture,
                PasswordHash = passwordHash
            };

            return await _userRepository.CreateUserAsync(user);
        }

        public async Task<User?> GetUserAsync(int id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<bool> UpdateUserAsync(int id, UserUpdateRequest request)
        {
            var user = await GetUserAsync(id);
            if (user == null) return false;

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.MobileNumber = request.MobileNumber;
            user.Language = request.Language;
            user.Culture = request.Culture;

            return await _userRepository.UpdateUserAsync(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _userRepository.DeleteUserAsync(id);
        }

        public bool ValidatePassword(User user, string password)
        {
            return user.PasswordHash == HashPassword(password);
        }

        public static string HashPassword(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return false;

            // Validate current password
            if (!ValidatePassword(user, currentPassword)) return false;

            // Hash the new password and update the user record
            user.PasswordHash = HashPassword(newPassword);
            return await _userRepository.UpdateUserAsync(user);
        }
    }
}
