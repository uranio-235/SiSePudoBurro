using Application.Behaviors;

using AspNetCore.Authentication.ApiKey;

using Coravel;

using Domain.Abstract.Services;
using Domain.Models;

using FluentValidation;

using Infrastructure.DAL;
using Infrastructure.Impl.Services;

using MediatR;

using Microsoft.OpenApi.Models;

using Polly;
using Polly.Extensions.Http;

using Swashbuckle.AspNetCore.SwaggerGen;

using System.Reflection;

using View.Middlewares;

namespace View;

// https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-6.0&tabs=visual-studio

public class Program
{
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

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddSwaggerGen();

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

        // coravel y sus amigos
        builder.Services.AddCache();
        builder.Services.AddQueue();

        // el sistema que notifica
        //builder.Services.AddTransient<ExceptionFilter>();

        // añade la capa de datos
        builder.Services.AddDataLayer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Payment API", Version = "v1" });
            c.AddSecurityDefinition("X-API-KEY", new OpenApiSecurityScheme()
            {
                Description = "El token de autenticación.",
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


        app.Run();
    }
}
