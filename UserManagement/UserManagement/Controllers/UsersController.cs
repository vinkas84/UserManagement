using Business.Interfaces;
using Domain.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace UserManagement.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserManager _userManager;

        public UserController(IUserManager userManager)
        {
            _userManager = userManager;
        }

        // Add a new user
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns validation errors
            }

            var createdUser = await _userManager.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUser), new { id = createdUser?.Id }, createdUser);
        }

        // Get user by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userManager.GetUserAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // Update user
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns validation errors
            }

            var updated = await _userManager.UpdateUserAsync(id, request);
            if (!updated) return BadRequest("Update failed.");

            return NoContent();
        }

        // Delete user
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleted = await _userManager.DeleteUserAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }

        // Validate user password
        [HttpPost("{id}/validate-password")]
        public async Task<IActionResult> ValidatePassword(int id, [FromBody] PasswordValidationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns validation errors
            }

            var user = await _userManager.GetUserAsync(id);
            if (user == null) return NotFound();

            var isValid = _userManager.ValidatePassword(user, request.Password);
            return Ok(new { Valid = isValid });
        }

        [HttpPut("{id}/update-password")]
        public async Task<IActionResult> UpdatePassword(int id, [FromBody] PasswordUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Return validation errors
            }

            var user = await _userManager.GetUserAsync(id);
            if (user == null) return NotFound();

            var passwordUpdated = await _userManager.UpdatePasswordAsync(id, request.CurrentPassword, request.NewPassword);
            
            return passwordUpdated ? Ok(new { Updated = true }) : Ok(new { Updated = false, Reason = "Incorrect current password or update failed." });
        }
    }
}
