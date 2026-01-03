using eLibrary.Api.Data;
using eLibrary.Api.Dtos;
using eLibrary.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eLibrary.Api.Services;

namespace eLibrary.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BorrowController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;



        public BorrowController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
        // ✅ Return a borrowed book
        [HttpPut("return/{borrowId}")]
        public async Task<IActionResult> ReturnBook(int borrowId, [FromQuery] int userId)
        {
            var borrow = await _context.Borrows
                .Include(b => b.Book)
                .Include(b => b.User)

                .FirstOrDefaultAsync(b => b.Id == borrowId && b.UserId == userId);

            if (borrow == null)
                return NotFound("Borrow record not found.");

            if (borrow.ReturnedAt != null)
                return BadRequest("This book is already returned.");

            borrow.ReturnedAt = DateTime.UtcNow;
            borrow.Book.AvailableCopies++;

            await _context.SaveChangesAsync();
            await _emailService.SendEmailAsync(
         borrow.User.Email,
         "Book Returned Successfully",
         $"Hello {borrow.User.Username},<br/><br/>" +
         $"You have successfully returned <strong>{borrow.Book.Title}</strong> on {DateTime.UtcNow:d}.<br/><br/>" +
         "Thank you for using eLibrary!"
         );
            return Ok(new
            {
                message = $"Book '{borrow.Book.Title}' returned successfully.",
                returnedAt = borrow.ReturnedAt,
                availableCopies = borrow.Book.AvailableCopies
            });
        }

        // ✅ Borrow a book 
        [HttpPost]
        public async Task<IActionResult> BorrowBook([FromBody] BorrowDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null) return NotFound("User not found");

             if (user.Role != "Member") return BadRequest("Only members can borrow books.");

            var book = await _context.Books.FindAsync(dto.BookId);
            if (book == null) return NotFound("Book not found");
            if (book.AvailableCopies <= 0) return BadRequest("No available copies");

            var startUtc = DateTime.SpecifyKind(dto.BorrowedAt, DateTimeKind.Utc);
            var dueUtc = DateTime.SpecifyKind(dto.DueAt, DateTimeKind.Utc);

            if (dueUtc <= startUtc)
                return BadRequest("Due date must be after start date.");

            if ((dueUtc - startUtc).TotalDays > 30)
                return BadRequest("Maximum loan length is 30 days.");

            var borrow = new Borrow
            {
                UserId = dto.UserId,
                BookId = dto.BookId,
                BorrowedAt = startUtc,
                DueAt = dueUtc
            };

            _context.Borrows.Add(borrow);
            book.AvailableCopies--;


            await _context.SaveChangesAsync();
            await _emailService.SendEmailAsync(user.Email,
                "Book Borrowed Confirmation",
                $"Hello {user.FullName},\n\nYou have successfully borrowed \"{book.Title}\".\nDue Date:{dueUtc:d}\n\nHappy reading!\n- eLibrary Team"
                );

            return Ok(new
            {
                message = $"Book '{book.Title}' borrowed successfully",
                bookTitle = book.Title,
                borrowId = borrow.Id,
                start = borrow.BorrowedAt,
                due = borrow.DueAt,
                availableCopies = book.AvailableCopies
            });
        }

        // ✅ Get all borrowed books for a user
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserBorrows(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");

            var borrows = await _context.Borrows
                .Include(b => b.Book)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BorrowedAt)
                .Select(b => new
                {
                    b.Id,
                    b.BookId,
                    b.Book.Title,
                    b.Book.Author,
                    b.BorrowedAt,
                    b.DueAt,
                    b.ReturnedAt,
                    Status = b.ReturnedAt == null
                        ? (b.DueAt < DateTime.UtcNow ? "Overdue" : "Borrowed")
                        : "Returned"
                })
                .ToListAsync();

            return Ok(borrows);
        }
    }
}
