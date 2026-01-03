using eLibrary.Api.Data;
using eLibrary.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eLibrary.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("User not found");

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.FullName,
                user.Role,
                user.CreatedAt
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updated)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("User not found");
            else
            {

                user.FullName = updated.FullName;
                user.Email = updated.Email;
                if (!string.IsNullOrWhiteSpace(updated.Password))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updated.Password);
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Profile updated successfully" });
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Borrows)      
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound("User not found");

            if (user.Borrows.Any())
                _context.Borrows.RemoveRange(user.Borrows);

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Account deleted successfully" });
        }
    }
    }

