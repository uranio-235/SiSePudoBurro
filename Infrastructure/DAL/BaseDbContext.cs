using Domain.Abstract.DAL;
using Domain.Entitities;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DAL;
public abstract class BaseDbContext : DbContext, IBaseDbContext
{
    public BaseDbContext(DbContextOptions<ReadDbContext> options) : base(options) { }
    public BaseDbContext(DbContextOptions<WriteDbContext> options) : base(options) { }

    public DbSet<Customer> Customer { get; set; }
    public DbSet<Balance> Balance { get; set; }
    public DbSet<Payment> Payment { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("uuid-ossp");

        modelBuilder
            .Entity<Customer>()
            .HasIndex(u => u.TelegramId)
            .IsUnique();

    } // OnModelCreating

}
