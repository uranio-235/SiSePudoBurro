namespace Application.Customers.Models;
public class Customer
{
    public Guid UserId { get; set; }
    public decimal AvailableBalance { get; set; } = 0;
    public decimal OutstandingBalance { get; set; } = 0;

    public string Username { get; set; }
    public string TelegramId { get; set; }

    public long TotalTransactions { get; set; } = 0;

    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
}
