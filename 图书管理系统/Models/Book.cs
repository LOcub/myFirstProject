namespace BookManager.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Isbn { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int AvailableQuantity { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime PublishDate { get; set; } = DateTime.Now;
        public string Description { get; set; } = string.Empty;
    }
}
