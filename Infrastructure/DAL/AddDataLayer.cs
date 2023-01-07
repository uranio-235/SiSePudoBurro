using Domain.Abstract.DAL;
using Domain.Entitities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Text;

namespace Infrastructure.DAL;
public static class AddDataLayerExtension
{
    /// <summary>
    /// Añade la capa de datos.
    /// </summary>
    public static IServiceCollection AddDataLayer(
            this IServiceCollection services)
    {
        var connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Demo;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        
        services.AddDbContext<WriteDbContext>(opt =>
            opt.UseSqlServer(
                connectionString,
                b => b.MigrationsAssembly("View")),
                ServiceLifetime.Scoped);

        services.AddDbContext<ReadDbContext>(opt =>
            opt.UseSqlServer(connectionString), ServiceLifetime.Scoped);

        services.AddScoped<IReadDbContext>(provider =>
            provider.GetService<ReadDbContext>());

        services.AddScoped<IWriteDbContext>(provider =>
            provider.GetService<WriteDbContext>());

        var dbContext = services
            .BuildServiceProvider()
            .GetService<WriteDbContext>();

        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        var telegramId = Convert.ToInt64(new Random().Next(1111111, 9999999));

        dbContext.Customer.Add(
            new Customer
            {
                View = new CustomerViewJson
                {
                    TelegramId = telegramId.ToString(),
                    Username = "LazaroArmando",
                    AvailableBalance = 0,
                    OutstandingBalance= 0,
                    TotalTransactions = 0
                },
                User = new User
                {
                    TelegramId = telegramId
                }
            });

        dbContext.SaveChanges();

        return services;
    }

}