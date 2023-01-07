using Domain.Abstract.DAL;
using Domain.Entitities;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DAL;
public abstract class BaseDbContext : DbContext, IBaseDbContext
{
    public BaseDbContext(DbContextOptions<ReadDbContext> options) : base(options) { }
    public BaseDbContext(DbContextOptions<WriteDbContext> options) : base(options) { }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("uuid-ossp");

        modelBuilder
            .Entity<User>()
            .HasIndex(u => u.TelegramId)
            .IsUnique();

        // https://devblogs.microsoft.com/dotnet/announcing-ef7-release-candidate-2/
        modelBuilder.Entity<Customer>().OwnsOne(
            cs => cs.View, builder =>
            {
                builder.ToJson();
                //builder.OwnsOne(contactDetails => contactDetails.Address);
            });

    } // OnModelCreating

}
