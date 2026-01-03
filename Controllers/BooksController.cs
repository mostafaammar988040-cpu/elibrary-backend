using eLibrary.Api.Data;
using eLibrary.Dtos;
using eLibrary.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eLibrary.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BooksController(AppDbContext context)
        {
            _context = context;
        }

        //  1. Get all books 
        [HttpGet]
        public async Task<IActionResult> GetBooks()
        {
            var books = await _context.Books
                .Include(b => b.Genre)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Genre = b.Genre.Name,
                    GenreId = b.GenreId,
                    AvailableCopies = b.AvailableCopies
                })
                .ToListAsync();

            return Ok(books);
        }

        //  2. Search & Filter endpoint
        [HttpGet("search")]
        public async Task<IActionResult> SearchBooks([FromQuery] string? title, [FromQuery] int? genreId)
        {
            var query = _context.Books
                .Include(b => b.Genre)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(b => b.Title.Contains(title) || b.Author.Contains(title));
            }

            if (genreId.HasValue)
            {
                query = query.Where(b => b.GenreId == genreId.Value);
            }

            var results = await query
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Genre = b.Genre.Name,
                    GenreId = b.GenreId,
                    AvailableCopies = b.AvailableCopies
                })
                .ToListAsync();

            return Ok(results);
        }

        //  3. Add new book (Admin only)
        [HttpPost]
        public async Task<IActionResult> AddBook([FromBody] BookDto dto, [FromQuery] int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");
            if (user.Role != "Admin") return Forbid("Only admins can add books.");

            var genreExists = await _context.Genres.AnyAsync(g => g.Id == dto.GenreId);
            if (!genreExists) return BadRequest("Invalid GenreId.");

            var book = new Book
            {
                Title = dto.Title,
                Author = dto.Author,
                GenreId = dto.GenreId,
                AvailableCopies = dto.AvailableCopies,
                TotalCopies = dto.AvailableCopies
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return Ok(book);
        }

        //  4. Delete book (Admin only)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id, [FromQuery] int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");
            if (user.Role != "Admin") return Forbid("Only admins can delete books.");

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound("Book not found.");

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Book '{book.Title}' deleted." });
        }
    }
}