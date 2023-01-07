using Application.Behaviors;

using AspNetCore.Authentication.ApiKey;

using Domain.Abstract.Services;
using Domain.Models;

using FluentValidation;

using Infrastructure.DAL;
using Infrastructure.Impl.Services;

using MassTransit;

using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.OpenApi.Models;

using Polly;
using Polly.Extensions.Http;

using Swashbuckle.AspNetCore.SwaggerGen;

using System.Reflection;

using View.Middlewares;

namespace View;

public class Program
{
    public static readonly InMemoryDatabaseRoot InMemoryDatabaseRoot = new InMemoryDatabaseRoot();
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        builder.Services.AddValidatorsFromAssembly(typeof(Application.ValidationHelper).Assembly);

        Assembly.GetAssembly(typeof(BaseConfig))
            ?.GetTypes()
            .Where(t => t is not null and { IsClass: true } and { IsAbstract: false })
            .Where(t => t.IsSubclassOf(typeof(BaseConfig)))
            .ToList()
            .ForEach(t =>
                builder.Services.AddSingleton(
                    t,
                    builder.Configuration.GetSection(t.Name.Replace("Config", string.Empty)).Get(t)));

        // el cliente de qvapay inyectado com ocliente http
        builder.Services.AddHttpClient<IQvapayClient, QvapayClient>()
                .AddPolicyHandler(
                   HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

        // Add services to the container.
        builder.Services.AddAuthorization();

        // volvieron a joder el swagger
        // esta pinga hay que ponerla para pder poner Command y Query como [FromBody] repetido
        builder.Services.AddSwaggerGen(o =>
            o.CustomSchemaIds(s => s!.FullName!.Replace("+", ".")));

        // Microsoft.AspNetCore.Mvc.NewtonsoftJson
        builder.Services
            .AddControllers(o =>
            {
                o.Filters.Add<ExceptionFilter>();
                o.Filters.Add<ResultFilter>();
            })
            .AddNewtonsoftJson();

        // carga el mediator y sus amigos
        builder.Services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());

        // los healtchecker
        builder.Services
            .AddHealthChecks()
            .AddCheck<IQvapayClient>("qvapay health check");

        // monta que te queda
        builder.Services.AddMassTransit(x =>
        {
            // la brujer�a que carga todo los consumer
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IConsumer).IsAssignableFrom(p) && p.IsClass && !p.FullName.StartsWith("MassTransit."))
                .ToList()
                .ForEach(c => x.AddConsumer(c));

            x.SetKebabCaseEndpointNameFormatter();

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });

        // a�ade la capa de datos
        builder.Services.AddDataLayer(InMemoryDatabaseRoot);

        // el swagger muy de pinga con autenticaci�n x-api-key sata
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Payment API", Version = "v1" });
            c.AddSecurityDefinition("X-API-KEY", new OpenApiSecurityScheme()
            {
                Description = "El token de autenticaci�n.",
                In = ParameterLocation.Header,
                Name = "X-API-KEY",
                Type = SecuritySchemeType.ApiKey
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "X-API-KEY" }
                        },
                        new string[] { }
                    }
                });

            c.IncludeXmlComments(
                Path.Combine(
                    AppContext.BaseDirectory,
                    $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));

            c.CustomOperationIds(desc => desc.TryGetMethodInfo(out var methodInfo) ? methodInfo.Name : default);

        });

        // el engendro para que swagger autentique
        builder.Services.AddTransient<ApiKeyProvider>();
        builder.Services
            .AddAuthentication(ApiKeyDefaults.AuthenticationScheme)
            .AddApiKeyInHeaderOrQueryParams<ApiKeyProvider>(options =>
            {
                options.Realm = "Encabezado X-API-KEY requerido";
                options.KeyName = "X-API-KEY";
            });

        var app = builder.Build();

        if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = string.Empty;
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPI v1");
            });
        }

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints => endpoints.MapHealthChecks("/health"));
        app.UseEndpoints(endpoints => endpoints.MapControllers());

        app.Run(); // me fui
    }
}
