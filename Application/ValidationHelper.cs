using FluentValidation;
using FluentValidation.Results;

using System.Net;

namespace Application;
public struct ValidationHelper
{
    /// <summary>
    /// Crear una exception de validación con el error y código http dado.
    /// </summary>
    /// <param name="error">el texto del error</param>
    /// <param name="httpStatus">opcionalmente el código http</param>
    /// <returns></returns>
    public static ValidationException CreateValidationException(
        string error,
        HttpStatusCode httpStatus = HttpStatusCode.BadRequest)
    {
        var errors = new ValidationFailure[]
        {
            new ValidationFailure("runtime",error)
            {
                ErrorCode = httpStatus.ToString()
            }
        };

        return new ValidationException(errors);
    }
}
