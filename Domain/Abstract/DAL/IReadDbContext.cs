using Domain.Entitities;

using Microsoft.EntityFrameworkCore;

namespace Domain.Abstract.DAL;
public interface IReadDbContext : IBaseDbContext
{
    /// <summary>
    /// Indica si la base de datos se puede conectar.
    /// </summary>
    bool IsOnline { get; }

    DbSet<Customer> Customer { get; set; } 
}
