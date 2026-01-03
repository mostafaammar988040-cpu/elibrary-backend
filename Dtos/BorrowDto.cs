namespace eLibrary.Api.Dtos
{
    public class BorrowDto
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime BorrowedAt { get; set; }
        public DateTime DueAt { get; set; }
    }
}
