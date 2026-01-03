using eLibrary.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eLibrary.Api.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminDashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalBooks = await _context.Books.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalBorrows = await _context.Borrows.CountAsync();

            var mostBorrowed = await _context.Borrows
                .GroupBy(b => b.Book.Title)
                .Select(g => new { BookTitle = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            return Ok(new
            {
                totalBooks,
                totalUsers,
                totalBorrows,
                mostBorrowed
            });
        }

        [HttpGet("reviews")]
        public async Task<IActionResult> GetAllReviews()
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Book)
                .Select(r => new {
                    r.Id,
                    UserName = r.User.Username,
                    BookTitle = r.Book.Title,
                    r.Rating,
                    r.Comment
                })
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return Ok(reviews);
        }


        [HttpDelete("reviews/{id}")]
        public IActionResult DeleteReview(int id)
        {
            var review = _context.Reviews.Find(id);
            if (review == null) return NotFound();
            _context.Reviews.Remove(review);
            _context.SaveChanges();
            return Ok();
        }
    }
}