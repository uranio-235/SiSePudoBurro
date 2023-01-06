using Domain.Entitities;

using Microsoft.EntityFrameworkCore;

namespace Domain.Abstract.DAL;
public interface IBaseDbContext
{
    /// <summary>
    /// Los clientes del negocio.
    /// </summary>
    DbSet<Customer> Customer { get; set; }

    /// <summary>
    /// La cantidad actual de dinero que tienen.
    /// </summary>
    DbSet<Balance> Balance { get; set; }

    /// <summary>
    /// Pagos, terminados o en curso.
    /// </summary>
    DbSet<Payment> Payment { get; set; }
}
