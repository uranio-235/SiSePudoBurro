using FluentResults;

using Microsoft.AspNetCore.Mvc;

namespace View.Api.V1;

public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Devuelve un enum contenido de un select. Cosas de frontend.
    /// </summary>
    internal OkObjectResult ResponseWithEnum<TEnum>(TEnum data) where TEnum : Enum
    {
        var keys = Enum.GetValues(data.GetType()).OfType<TEnum>().Select(e => e.ToString());
        var values = Enum.GetValuesAsUnderlyingType(data.GetType()).OfType<int>();

        var dict = keys
            .Zip(values, (k, v) => new KeyValuePair<string, int>(k, v))
            .ToDictionary();

        return new OkObjectResult(dict);
    }


    /// <summary>
    /// Devuelve un Result como un BadRequest listo para estampárselo a asp.net en toda su cara.
    /// Primer error es el detalle, segundo error es el status code, tercer error es el title.
    /// </summary>
    /// <param name="statusCode">si deseas dar un status code diferente</param>
    /// <returns>Un BadRequest listo para estampárselo a asp.net en toda su cara.</returns>
    /// <exception cref="ArgumentException">Si no se respeta el orden de los errores.</exception>
    internal ObjectResult ResponseWithBadRequest(Result result, int statusCode = StatusCodes.Status400BadRequest)
    {
        if (result is { IsSuccess: true })
            throw new ArgumentException("Result no parece un error.");

        var errors = result.Errors.Select(e => e.Message).ToList();

        if (errors is { Count: 0 } or { Count: > 3 })
            throw new ArgumentException(
                "Primer mensaje error details, segundo mensaje status code, tercer mensaje title.");

        var details = result.Errors.Count > 0 ? errors.First() : string.Empty;

        var hasStatusCode = int.TryParse(errors.Skip(1).Take(1).SingleOrDefault() ?? statusCode.ToString(), out int sc);
        var finalStatusCode = hasStatusCode && sc > 0 ? sc : statusCode;

        var title = errors.Skip(2).Take(1).SingleOrDefault() ?? "ERROR";

        return new(
            new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                Detail = details,
                Status = finalStatusCode != 0 ? finalStatusCode : statusCode,
                Title = title

            })
        {
            StatusCode = finalStatusCode
        };
    }

    /// <inheritdoc cref="ResponseWithBadRequest(Result, int)"/>
    internal ObjectResult ResponseWithBadRequest<T>(Result<T> result, int statusCode = StatusCodes.Status400BadRequest)
        => ResponseWithBadRequest(result, statusCode);
}