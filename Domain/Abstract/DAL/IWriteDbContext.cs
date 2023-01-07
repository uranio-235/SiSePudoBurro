using Domain.Entitities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Domain.Abstract.DAL;
public interface IWriteDbContext : IBaseDbContext
{
    /// <summary>
    /// Inicia una operación transasicional.
    /// </summary>
    /// <returns>la transaction</returns>
    Task<IDbContextTransaction> BeginTransactionAsync();

    /// <summary>
    /// Salva cambios de manera sincrónica.
    /// </summary>
    /// <returns>la cantidad de entidades afectadas</returns>
    int SaveChanges();

    /// <summary>
    /// Salva cambios de manera asíncrona.
    /// </summary>
    /// <returns>la cantidad de entidades afectadas</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Los clientes del negocio.
    /// </summary>
    DbSet<User> User { get; set; }

    /// <summary>
    /// Pagos, terminados o en curso.
    /// </summary>
    DbSet<Payment> Payment { get; set; }

    /// <summary>
    /// El estado actual del cliente, incluye su balance.
    /// </summary>
    DbSet<Customer> Customer { get; set; }
}
