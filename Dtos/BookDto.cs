namespace eLibrary.Dtos
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public int AvailableCopies { get; set; }
        public int GenreId { get; set; }
        public string? Genre { get; set; }
        public int TotalCopies { get; set; }

    }
}
