using System;

namespace eLibrary.Api.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        // Foreign key to User
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Foreign key to Book
        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        public string? BookName { get; set; }

        // When the user added this book to favorites
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}