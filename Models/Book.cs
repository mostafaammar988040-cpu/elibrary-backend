using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace eLibrary.Api.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public int GenreId { get; set; }
        public Genre Genre { get; set; } = null!;
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public ICollection<Borrow> Borrows { get; set; } = new List<Borrow>();
        public String Language { get; set; } = "EN";//EN,AR,FR
    }
}
