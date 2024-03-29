﻿namespace Domain.Entitities;
public class Customer : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; }

    public CustomerViewJson View { get; set; } = new();
}

public class CustomerViewJson
{
    public decimal AvailableBalance { get; set; } = 0;
    public decimal OutstandingBalance { get; set; } = 0;

    public string Username { get; set; }
    public string TelegramId { get; set; }

    public long TotalTransactions { get; set; } = 0;
}