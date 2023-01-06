using Domain.SharedKernel;

namespace Domain.Entitities;
public class Payment : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; }

    public PaymentProvider Provider { get; set; }

    public PaymentStatus Status { get; set; }

    public LocalCurrency Currency { get; set; }

    public string ExternalIdentification { get; set; }

    public decimal RequestedAmount { get; set; }
    public decimal FinalOutstandingAmount { get; set; } = 0;
}
