using Domain.Abstract.DAL;
using Domain.Entitities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.DAL;
public class WriteDbContext : BaseDbContext, IWriteDbContext
{
    public WriteDbContext(DbContextOptions<WriteDbContext> options) : base(options) { }

    public override int SaveChanges() => SaveChangesAsync().Result;

    public Task<IDbContextTransaction> BeginTransactionAsync() =>
        Database.BeginTransactionAsync();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // la brujería que actualiza las fechas de creación y modificación

        ChangeTracker.Entries<BaseEntity>()
            .Where(entity => entity.State == EntityState.Added)
            .ToList().ForEach(item => item.Entity.CreatedAt = item.Entity.ModifiedAt = now);

        ChangeTracker.Entries<BaseEntity>()
            .Where(entity => entity.State == EntityState.Modified)
            .ToList().ForEach(item => item.Entity.ModifiedAt = now);

        return await base.SaveChangesAsync(cancellationToken);

    } // method

    public DbSet<User> User { get; set; }
    public DbSet<Payment> Payment { get; set; }
    public DbSet<Customer> Customer { get; set; }

} // class

