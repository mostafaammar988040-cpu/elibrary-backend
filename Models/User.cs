namespace eLibrary.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public ICollection<Borrow> Borrows { get; set; } = new List<Borrow>();
        public string Role { get; set; } = "Member";
        public string? AvatarUrl { get; set; }

    }
}
