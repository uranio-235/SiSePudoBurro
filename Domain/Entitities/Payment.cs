using Domain.SharedKernel;

namespace Domain.Entitities;
public class Payment : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; }

    public PaymentProvider Provider { get; set; }

    public PaymentStatus Status { get; set; }

    public LocalCurrency Currency { get; set; }

    public decimal RequestedAmount { get; set; }
    public decimal ExecutedAmount { get; set; } = 0;

    public string ExternalIdentification { get; set; }
}
