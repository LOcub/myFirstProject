namespace BookManager.Models
{
    public class BorrowRecord
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime BorrowDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = "borrowed";
        public string UserName { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
    }
}
