using Business.Interfaces;
using Domain.DTOs;
using Domain.Models;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Controllers;
using Xunit;

namespace UserManagementAPITests
{
    public class UserControllerTests
    {
        private readonly Mock<IUserManager> _mockUserManager;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockUserManager = new Mock<IUserManager>();
            _controller = new UserController(_mockUserManager.Object);
        }

        [Fact]
        public async Task CreateUser_ValidRequest_ReturnsCreatedAtAction()
        {
            // Arrange
            var request = new UserCreateRequest { UserName = "TestUser", Email = "test@example.com", Password = "TestPass" };
            var response = new User { Id = 1, UserName = "TestUser", Email = "test@example.com", PasswordHash = UserManager.HashPassword("TestPass") };
            _mockUserManager.Setup(m => m.CreateUserAsync(request)).ReturnsAsync(response);

            // Act
            var result = await _controller.CreateUser(request);

            // Assert
            var createdAtAction = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<User>(createdAtAction.Value);
            Assert.Equal(1, returnValue.Id);
        }

        [Fact]
        public async Task CreateUser_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var request = new UserCreateRequest { UserName = "TestUser", Email = "testexamplecom", Password = "TestPass" }; // Missing required fields
            _controller.ModelState.AddModelError("Email", "Invalid email format");

            // Act
            var result = await _controller.CreateUser(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetUser_UserExists_ReturnsOk()
        {
            // Arrange
            var userId = 1;
            var user = new User { Id = userId, UserName = "TestUser", Email = "test@example.com", PasswordHash = UserManager.HashPassword("TestPass") };
            _mockUserManager.Setup(m => m.GetUserAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<User>(okResult.Value);
            Assert.Equal(userId, returnValue.Id);
        }

        [Fact]
        public async Task GetUser_UserDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<int>())).ReturnsAsync((User)null);

            // Act
            var result = await _controller.GetUser(99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateUser_ValidRequest_ReturnsNoContent()
        {
            // Arrange
            var request = new UserUpdateRequest { FullName = "Updated Name", Email = "test@example.com" };
            _mockUserManager.Setup(m => m.UpdateUserAsync(It.IsAny<int>(), request)).ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateUser(1, request);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateUser_FailedUpdate_ReturnsBadRequest()
        {
            // Arrange
            var request = new UserUpdateRequest { FullName = "Updated Name", Email = "test@example.com" };
            _mockUserManager.Setup(m => m.UpdateUserAsync(It.IsAny<int>(), request)).ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateUser(1, request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteUser_UserExists_ReturnsNoContent()
        {
            // Arrange
            _mockUserManager.Setup(m => m.DeleteUserAsync(It.IsAny<int>())).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteUser(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUser_UserDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _mockUserManager.Setup(m => m.DeleteUserAsync(It.IsAny<int>())).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteUser(99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ValidatePassword_ValidUser_ReturnsOkWithValidationResult()
        {
            // Arrange
            var userId = 1;
            var request = new PasswordValidationRequest { Password = "password123" };
            var user = new User { Id = userId, UserName = "TestUser", Email = "test@example.com", PasswordHash = UserManager.HashPassword("TestPass") };
            _mockUserManager.Setup(m => m.GetUserAsync(userId)).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.ValidatePassword(user, request.Password)).Returns(true);

            // Act
            var result = await _controller.ValidatePassword(userId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result); 
            var value = okResult.Value?.ToString();
            Assert.Contains("Valid = True", value);
        }

        [Fact]
        public async Task UpdatePassword_ValidRequest_ReturnsOkWithUpdatedTrue()
        {
            // Arrange
            var userId = 1;
            var request = new PasswordUpdateRequest { CurrentPassword = "oldPass", NewPassword = "newPass" };
            var user = new User { Id = userId, UserName = "TestUser", Email = "test@example.com", PasswordHash = UserManager.HashPassword("oldPass") };

            _mockUserManager.Setup(m => m.GetUserAsync(userId)).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.UpdatePasswordAsync(userId, request.CurrentPassword, request.NewPassword))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdatePassword(userId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result); 
            var value = okResult.Value?.ToString();
            Assert.Contains("Updated = True", value);
        }

        [Fact]
        public async Task UpdatePassword_InvalidPassword_ReturnsOkWithUpdatedFalseAndReason()
        {
            // Arrange
            var userId = 1;
            var request = new PasswordUpdateRequest { CurrentPassword = "wrongPass", NewPassword = "newPass" };
            var user = new User { Id = userId, UserName = "TestUser", Email = "test@example.com", PasswordHash = UserManager.HashPassword("somePass") };

            _mockUserManager.Setup(m => m.GetUserAsync(userId)).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.UpdatePasswordAsync(userId, request.CurrentPassword, request.NewPassword))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UpdatePassword(userId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value?.ToString();
            Assert.Contains("Updated = False, Reason = Incorrect current password or update failed.", value);
        }

        [Fact]
        public async Task UpdatePassword_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = 99;
            var request = new PasswordUpdateRequest { CurrentPassword = "oldPass", NewPassword = "newPass" };

            _mockUserManager.Setup(m => m.GetUserAsync(userId)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.UpdatePassword(userId, request);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdatePassword_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var userId = 1;
            _controller.ModelState.AddModelError("NewPassword", "Password is required.");

            // Act
            var result = await _controller.UpdatePassword(userId, new PasswordUpdateRequest { CurrentPassword = "oldPass", NewPassword = "newPass" });

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
