using Xunit;
using Microsoft.EntityFrameworkCore;
using DataAccess.Repositories;
using Domain.Models;
using System.Threading.Tasks;
using DataAccess;
using Infrastructure;

namespace DataAccessTests
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

            _context = new ApplicationDbContext(options);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted(); // Clears the in-memory database
            _context.Dispose();
        }

        [Fact]
        public async Task CreateUserAsync_ValidUser_ReturnsUser()
        {
            var repository = new UserRepository(_context);
            var user = new User { Id = 1, UserName = "TestUser", Email = "test@example.com", PasswordHash = UserManager.HashPassword("TestPass") };

            var result = await repository.CreateUserAsync(user);

            Assert.NotNull(result);
            Assert.Equal("TestUser", result.UserName);
        }

        [Fact]
        public async Task GetUserByIdAsync_UserExists_ReturnsUser()
        {
            var repository = new UserRepository(_context);
            var user = new User { Id = 1, UserName = "TestUser", Email = "test@example.com", PasswordHash = UserManager.HashPassword("TestPass") };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await repository.GetUserByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetUserByIdAsync_UserDoesNotExist_ReturnsNull()
        {
            var repository = new UserRepository(_context);
            var result = await repository.GetUserByIdAsync(99);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateUserAsync_ValidUser_ReturnsTrue()
        {
            var repository = new UserRepository(_context);
            var user = new User { Id = 1, UserName = "TestUser", Email = "test@example.com", PasswordHash = UserManager.HashPassword("TestPass") };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            user.UserName = "UpdatedUser";
            var result = await repository.UpdateUserAsync(user);

            Assert.True(result);
            var updatedUser = await repository.GetUserByIdAsync(1);
            Assert.Equal("UpdatedUser", updatedUser?.UserName);
        }

        [Fact]
        public async Task DeleteUserAsync_UserExists_ReturnsTrue()
        {
            var repository = new UserRepository(_context);
            var user = new User { Id = 1, UserName = "TestUser", Email = "test@example.com", PasswordHash = UserManager.HashPassword("TestPass") };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await repository.DeleteUserAsync(1);

            Assert.True(result);
            var deletedUser = await repository.GetUserByIdAsync(1);
            Assert.Null(deletedUser);
        }

        [Fact]
        public async Task DeleteUserAsync_UserDoesNotExist_ReturnsFalse()
        {
            var repository = new UserRepository(_context);
            var result = await repository.DeleteUserAsync(99);

            Assert.False(result);
        }
    }
}
