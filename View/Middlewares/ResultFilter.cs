using FluentResults;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using Newtonsoft.Json;

namespace View.Middlewares;

public class ResultFilter : IAsyncResultFilter
{
    private sealed class Response
    {
        public object? Value { get; set; }
        public bool? IsSuccess { get; set; }
        public bool? IsFailed { get; set; }

        public string[] Errors { get; set; } = new string[] { };
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var json = context?.Result?.GetType()?.Name switch
        {
            nameof(OkResult) => JsonConvert.SerializeObject(Result.Ok()),
            nameof(OkObjectResult) => JsonConvert.SerializeObject((context.Result as OkObjectResult).Value),
            _ => JsonConvert.SerializeObject(Result.Fail($"No hay filtros de salida definido para el tipo {context.Result.GetType().Name}"))
        } ?? JsonConvert.SerializeObject(Result.Fail("el context no tiene resultados"));

        var data = JsonConvert.DeserializeObject<Response>(json);
        context.Result = new OkObjectResult(data);

        await next();
    }
}

/*
       SELECT TOP(2) CAST(JSON_VALUE([c].[View],'$.AvailableBalance') AS decimal(18,2)) AS [AvailableBalance], CAST(JSON_VALUE([c].[View],'$.OutstandingBalance') AS decimal(18,2)) AS [OutstandingBalance], CAST(JSON_VALUE([c].[View],'$.TelegramId') AS nvarchar(max)) AS [TelegramId], CAST(JSON_VALUE([c].[View],'$.TotalTransactions') AS bigint) AS [TotalTransactions], [c].[UserId], CAST(JSON_VALUE([c].[View],'$.Username') AS nvarchar(max)) AS [Username]
      FROM [Customer] AS [c]
      WHERE [c].[UserId] = @__request_UserId_0
*/