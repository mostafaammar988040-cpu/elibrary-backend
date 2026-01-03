using System;

namespace eLibrary.Api.Models
{
    public class Borrow
    {
        public int Id { get; set; }

        // 🔑 Foreign key to User
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // 🔑 Foreign key to Book
        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        public DateTime BorrowedAt { get; set; } = DateTime.UtcNow;
        public DateTime DueAt { get; set; }
        public DateTime? ReturnedAt { get; set; }
    }
}