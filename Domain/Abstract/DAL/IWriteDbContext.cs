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
}
