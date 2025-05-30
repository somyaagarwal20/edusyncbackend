using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSyncwebapi.Data;
using EduSyncwebapi.Models;
using EduSyncwebapi.Dtos;

namespace EduSyncwebapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserModelsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserModelsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UserModels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUserModels()
        {
            var users = await _context.UserModels.ToListAsync();

            // Convert UserModel list to UserDto list
            var userDtos = users.Select(u => new UserDto
            {
                UserId = u.UserId,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                Password = null // Do not expose password
            }).ToList();

            return Ok(userDtos);
        }

        // GET: api/UserModels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserModel(Guid id)
        {
            var user = await _context.UserModels.FindAsync(id);

            if (user == null)
                return NotFound();

            var userDto = new UserDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                Password = null // Do not expose password
            };

            return Ok(userDto);
        }

        // PUT: api/UserModels/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserModel(Guid id, UserDto userDto)
        {
            if (id != userDto.UserId)
                return BadRequest();

            var user = await _context.UserModels.FindAsync(id);
            if (user == null)
                return NotFound();

            // Update properties
            user.Name = userDto.Name;
            user.Email = userDto.Email;
            user.Role = userDto.Role;

            // Optional: Update password hash here if Password is not null
            if (!string.IsNullOrWhiteSpace(userDto.Password))
            {
                user.PasswordHash = HashPassword(userDto.Password); // Implement this method
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserModelExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/UserModels
        [HttpPost]
        public async Task<ActionResult<UserDto>> PostUserModel(UserDto userDto)
        {
            var user = new UserModel
            {
                UserId = Guid.NewGuid(),
                Name = userDto.Name,
                Email = userDto.Email,
                Role = userDto.Role,
                PasswordHash = HashPassword(userDto.Password) // Implement this securely
            };

            _context.UserModels.Add(user);
            await _context.SaveChangesAsync();

            // Return the DTO (without password)
            userDto.UserId = user.UserId;
            userDto.Password = null;

            return CreatedAtAction(nameof(GetUserModel), new { id = user.UserId }, userDto);
        }

        // DELETE: api/UserModels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserModel(Guid id)
        {
            var user = await _context.UserModels.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.UserModels.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/UserModels/login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var user = await _context.UserModels
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(new LoginResponse
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            });
        }

        // POST: api/UserModels/reset-password
        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var user = await _context.UserModels
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                return NotFound(new { message = "User not found" });

            // Update password
            user.PasswordHash = HashPassword(request.Password);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password reset successful" });
        }

        private bool UserModelExists(Guid id)
        {
            return _context.UserModels.Any(e => e.UserId == id);
        }

        // Dummy password hash method (replace with secure one like BCrypt)
        private string HashPassword(string? password)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password ?? ""));
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
