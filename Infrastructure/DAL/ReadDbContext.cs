using Domain.Abstract.DAL;
using Domain.Entitities;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DAL;
public class ReadDbContext : BaseDbContext, IReadDbContext
{
    bool IReadDbContext.IsOnline => this.Database.CanConnect();

    public ReadDbContext(DbContextOptions<ReadDbContext> options) : base(options)
    {
        // el dbcontext de lectura siempre hace las consultas as no tracking
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public DbSet<Customer> Customer { get; set; }
}
