using eLibrary.Api.Data;
using eLibrary.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eLibrary.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavoritesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FavoritesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetFavorites(int userId)
        {
            var favorites = await _context.Favorites
                .Include(f => f.Book)
                .Where(f => f.UserId == userId)
                .Select(f => new
                {
                    f.Id,
                    f.BookId,
                    f.BookName,                
                    Title = f.Book.Title,     
                    Author = f.Book.Author,
                    f.AddedAt
                })
                .ToListAsync();

            return Ok(favorites);
        }

        [HttpPost("{userId}/{bookId}")]
        public async Task<IActionResult> AddFavorite(int userId, int bookId)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                return NotFound("User not found.");

            var book = await _context.Books
                        .FirstOrDefaultAsync(b => b.Id == bookId);
            if (book == null)
                return NotFound("Book not found.");

            // Avoid duplicates
            var alreadyExists = await _context.Favorites
                .AnyAsync(f => f.UserId == userId && f.BookId == bookId);
            if (alreadyExists)
                return BadRequest("This book is already in favorites.");

            var favorite = new Favorite
            {
                UserId = userId,
                BookId = bookId,
                BookName = book.Title,  
                AddedAt = DateTime.UtcNow
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Book added to favorites!" });
        }

        // DELETE api/Favorites/{userId}/{bookId}
        [HttpDelete("{userId:int}/{bookId:int}")]
        public async Task<IActionResult> RemoveFavorite(int userId, int bookId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.BookId == bookId);

            if (favorite == null)
                return NotFound("Favorite not found.");

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Removed from favorites." });
        }
    }
}