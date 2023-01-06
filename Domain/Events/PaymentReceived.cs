namespace Domain.Events;

/// <summary>
/// Evento desencadenado cuando llega un pago.
/// </summary>
public class PaymentReceived
{
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
}
