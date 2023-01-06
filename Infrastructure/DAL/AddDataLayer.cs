using Domain.Abstract.DAL;

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
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var container = services.BuildServiceProvider();

        var exists = Uri.TryCreate(
            Environment.GetEnvironmentVariable("DATABASE_URL"),
            UriKind.Absolute, out var databaseUrl);

        if (!exists)
            throw new InvalidProgramException("La variable DATABASE_URL no está declarada.");

        var connectionString = new StringBuilder();
        connectionString.Append($"Server={databaseUrl.Host};");
        connectionString.Append(@"Port=5432;");
        connectionString.Append($"Database={databaseUrl.LocalPath.Substring(1)};");
        connectionString.Append($"User Id={databaseUrl.UserInfo.Split(':')[0]};");
        connectionString.Append($"Password={databaseUrl.UserInfo.Split(':')[1]};");
        connectionString.Append(@"SSL Mode=Require;");
        connectionString.Append(@"Trust Server Certificate=true;");

        container.GetService<ILogger<WriteDbContext>>()
            .LogInformation("cadena de conexión resuelta: «{E}»", connectionString.ToString());

        services.AddDbContext<WriteDbContext>(opt =>
            opt.UseNpgsql(
                connectionString.ToString(),
                b => b.MigrationsAssembly("PaymentGateway")),
                ServiceLifetime.Scoped);

        services.AddDbContext<ReadDbContext>(opt =>
            opt.UseNpgsql(connectionString.ToString()), ServiceLifetime.Scoped);

        services.AddScoped<IReadDbContext>(provider =>
            provider.GetService<ReadDbContext>());

        services.AddScoped<IWriteDbContext>(provider =>
            provider.GetService<WriteDbContext>());

        return services;
    }

}