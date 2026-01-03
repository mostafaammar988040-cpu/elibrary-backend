namespace eLibrary.Api.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string BookTitle { get; set; }
        public string ExternalId { get; set; }
        public decimal Price { get; set; }
    }
}

