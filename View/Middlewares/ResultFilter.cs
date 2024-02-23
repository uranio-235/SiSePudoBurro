using FluentResults;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using System.Net;
using System.Text.Json;

namespace View.Middlewares;

public class ResultFilter : IAsyncResultFilter
{
    private sealed class Response
    {
        public object? Value { get; set; }
        public bool IsSuccess { get; set; } = false;
        public bool IsFailed { get; set; } = true;

        public string[] Errors { get; set; } = [];
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult)
        {
            var data = context.Result switch
            {
                OkObjectResult => context.Result as OkObjectResult,
                BadRequestObjectResult => context.Result as BadRequestObjectResult,
                _ => context.Result as ObjectResult
            };

            if (data?.Value?.GetType()?.BaseType?.BaseType?.FullName == "FluentResults.ResultBase")
            {
                var result = Result.Merge((ResultBase)data.Value);

                if (result.IsFailed)
                {
                    context.Result = new BadRequestObjectResult(
                        new Response
                        {
                            IsFailed = true,
                            IsSuccess = false,
                            Errors = result.Errors.Select(e => e.Message).ToArray()
                        });

                    context.HttpContext.Response.StatusCode =
                        result.Errors.First().Message.ToLowerInvariant() switch
                        {
                            string s when s.Contains("not found") => HttpStatusCode.NotFound.GetHashCode(),
                            string s when s.Contains("forbidden") => HttpStatusCode.NotFound.GetHashCode(),
                            _ => HttpStatusCode.BadRequest.GetHashCode()
                        };
                }
                else if (result is { IsSuccess: true })
                {
                    context.Result = new OkObjectResult(JsonSerializer.Deserialize<Response>(JsonSerializer.Serialize(data.Value)));
                }
            }
            else
            {
                context.Result = new BadRequestObjectResult(
                    new Response
                    {
                        IsFailed = true,
                        IsSuccess = false,
                        Errors = ["La respuesta no fue un Result<T>. Arregla esa mierda anda."]
                    });

                context.HttpContext.Response.StatusCode = HttpStatusCode.UnprocessableContent.GetHashCode();
            }
        }

        await next();
    }
}
