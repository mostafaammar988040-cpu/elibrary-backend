using eLibrary.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace eLibrary.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RecommendationsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetForUser(int userId)
        {
            var favoriteGenres = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Join(_context.Books,
                      fav => fav.BookId,
                      book => book.Id,
                      (fav, book) => book.GenreId)
                .ToListAsync();

            if (favoriteGenres.Count == 0)
            {
                var starterBooks = await _context.Books
                    .Include(b => b.Genre)
                    .OrderBy(b => Guid.NewGuid())
                    .Take(5)
                    .Select(b => new
                    {
                        b.Id,
                        b.Title,
                        b.Author,
                        GenreName = b.Genre != null ? b.Genre.Name : "Unknown",
                        b.AvailableCopies
                    })
                    .ToListAsync();

                return Ok(starterBooks);
            }

            var topGenres = favoriteGenres
                .GroupBy(g => g)
                .OrderByDescending(g => g.Count())
                .Take(2)
                .Select(g => g.Key)
                .ToList();

            var recommendations = await _context.Books
                .Include(b => b.Genre)
                .Where(b => topGenres.Contains(b.GenreId))
                .Where(b => !_context.Favorites
                    .Any(f => f.UserId == userId && f.BookId == b.Id))
                .OrderBy(b => Guid.NewGuid())
                .Take(10)
                .Select(b => new
                {
                    b.Id,
                    b.Title,
                    b.Author,
                    GenreName = b.Genre != null ? b.Genre.Name : "Unknown",
                    b.AvailableCopies
                })
                .ToListAsync();

           
            if (recommendations.Count == 0)
            {
                recommendations = await _context.Books
                    .Include(b => b.Genre)
                    .Where(b => !_context.Favorites
                        .Any(f => f.UserId == userId && f.BookId == b.Id))
                    .OrderBy(b => Guid.NewGuid())
                    .Take(5)
                    .Select(b => new
                    {
                        b.Id,
                        b.Title,
                        b.Author,
                        GenreName = b.Genre != null ? b.Genre.Name : "Unknown",
                        b.AvailableCopies
                    })
                    .ToListAsync();
            }

            return Ok(recommendations);
        }
    }
}