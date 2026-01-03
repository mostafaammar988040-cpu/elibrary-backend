using Microsoft.EntityFrameworkCore;
using eLibrary.Api.Models;

namespace eLibrary.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Genre> Genres => Set<Genre>();
        public DbSet<Book> Books => Set<Book>();
        public DbSet<Favorite> Favorites => Set<Favorite>();
        public DbSet<Borrow> Borrows => Set<Borrow>();
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

    }

}
