using FluentResults;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using System.Net;

namespace View.Middlewares;

public class ExceptionFilter : ExceptionFilterAttribute
{
    private readonly ILogger<ExceptionFilter> _logger;

    public ExceptionFilter(
        ILogger<ExceptionFilter> logger)
    {
        _logger = logger;
    }


    public override void OnException(ExceptionContext context)
    {
        var exceptions = GetExceptions(context.Exception);

        exceptions
            .Select(e => new
            {
                Exception = e,
                IsValidation = e is ValidationException
            })
            .Select(e => new
            {
                e.Exception,
                e.IsValidation,
                LogLevel = e.IsValidation ? LogLevel.Information : LogLevel.Critical
            })
            .ToList()
            .ForEach(col =>
                _logger.Log(
                    col.LogLevel,
                    $"Exception {(col.IsValidation ? "de validación" : "no manejada")}",
                    col.Exception));

        var error = exceptions
                .Where(ex => ex is ValidationException and not null)
                .Select(ex => ex as ValidationException)
                .SelectMany(ex => ex.Errors)
                .Select(e => new
                {
                    e.ErrorCode,
                    e.ErrorMessage
                })
                .Select(e =>
                    Enum.TryParse<HttpStatusCode>(e.ErrorCode, out var result) ?
                    new { e.ErrorMessage, ErrorCode = result } :
                    new { e.ErrorMessage, ErrorCode = HttpStatusCode.InternalServerError }
                 )
                .FirstOrDefault();

        context.HttpContext.Response.StatusCode =
            (int)(error?.ErrorCode ?? HttpStatusCode.InternalServerError);

        if (error is null)
        {
            exceptions?
                .Select((e, n) => new { e.Message, Index = n })
                .ToList()
                .ForEach(e => _logger.LogError("{Index} {Message}", e.Message, e.Index));

            _logger.LogCritical("Palo fulísima:\n{Exception}", context.Exception);
        }

        context.Result = new ObjectResult(Result.Fail(error?.ErrorMessage ?? "(unkown error)"));

        base.OnException(context);
    }

    /// <summary>
    /// Toma las exception de manera recursiva y devuelve una lista con el mensaje y tipo.
    /// </summary>
    /// <param name="ex">una exception con inners exceptions</param>
    /// <returns>el nombre y los message de las exception</returns>
    public IList<Exception> GetExceptions(Exception ex)
        => ex.InnerException == default || string.IsNullOrEmpty(ex.InnerException.Message) ?
            new Exception[] { ex } :
                new Exception[] { ex }
                    .ToList()
                    .Concat(GetExceptions(ex.InnerException)).ToList();

} // class
