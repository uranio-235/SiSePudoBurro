using Domain.SharedKernel;

namespace Domain.Entitities;
public class Balance : BaseEntity
{
    /// <summary>
    /// La Fk del cliente.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// El cliente.
    /// </summary>
    public Customer Customer { get; set; }

    /// <summary>
    /// Suma total del balance de usuario.
    /// </summary>
    public decimal AvailableBalance { get; set; }

    /// <summary>
    /// Que moneda representa este balance.
    /// </summary>
    public LocalCurrency Currency { get; set; }
}
