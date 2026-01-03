using eLibrary.Api.Data;
using eLibrary.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eLibrary.Api.Controllers
{
    public class CreateOrderItemDto
    {
        public string BookTitle { get; set; } = "";
        public string ExternalId { get; set; } = "";
        public decimal Price { get; set; }
    }

    public class CreateOrderDto
    {
        public int UserId { get; set; }
        public string PaymentMethod { get; set; } = "Card";
        public decimal TotalPrice { get; set; }
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            if (dto.Items == null || dto.Items.Count == 0)
                return BadRequest("No items provided.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
            if (user == null) return BadRequest("User not found.");

            var computed = dto.Items.Sum(i => i.Price);
            if (computed != dto.TotalPrice)
            {
                if (Math.Abs(computed - dto.TotalPrice) > 0.01m)
                    return BadRequest("Total does not match items sum.");
            }

            var order = new Order
            {
                UserId = dto.UserId,
                PaymentMethod = dto.PaymentMethod,
                TotalPrice = dto.TotalPrice,
                CreatedAt = DateTime.UtcNow
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var it in dto.Items)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    BookTitle = it.BookTitle,
                    ExternalId = it.ExternalId ?? "",
                    Price = it.Price
                });
            }
            await _context.SaveChangesAsync();


            return Ok(new { order.Id, order.TotalPrice, order.CreatedAt });
        }
    }
}