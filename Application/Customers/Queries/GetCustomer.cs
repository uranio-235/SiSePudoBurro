using Application.Customers.Models;

using Domain.Abstract.DAL;

using FluentResults;

using MediatR;

using System.Net;

namespace Application.Customers.Queries;

public class GetCustomer
{

    public record Query(Guid UserId) : IRequest<Result<Customer>>;

    public class QueryHandler : IRequestHandler<Query, Result<Customer>>
    {
        private readonly IReadDbContext _dbContext;

        public QueryHandler(IReadDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<Customer>> Handle(Query request, CancellationToken cancellationToken)
        {
            var customer = _dbContext.Customer
                .Select(c => new Customer
                {
                    AvailableBalance = c.View.AvailableBalance,
                    OutstandingBalance = c.View.OutstandingBalance,
                    TelegramId = c.View.TelegramId,
                    TotalTransactions = c.View.TotalTransactions,
                    UserId = c.UserId,
                    Username = c.View.Username,
                    CreatedAt = c.CreatedAt,
                    ModifiedAt = c.ModifiedAt
                })
                .SingleOrDefault(c => c.UserId == request.UserId);

            if (customer == default)
                throw ValidationHelper.CreateValidationException(
                    error: "No existe ningún usuario con esa id",
                    httpStatus: HttpStatusCode.NotFound);

            return customer;
        }
    }

} // main class 

/*
SELECT TOP(2) 
CAST(JSON_VALUE([c].[View],'$.AvailableBalance') AS decimal(18,2)) AS [AvailableBalance],
CAST(JSON_VALUE([c].[View],'$.OutstandingBalance') AS decimal(18,2)) AS [OutstandingBalance],
CAST(JSON_VALUE([c].[View],'$.TelegramId') AS nvarchar(max)) AS [TelegramId],
CAST(JSON_VALUE([c].[View],'$.TotalTransactions') AS bigint) AS [TotalTransactions], [c].[UserId],
CAST(JSON_VALUE([c].[View],'$.Username') AS nvarchar(max)) AS [Username]
FROM [Customer] AS [c]
WHERE [c].[UserId] = @__request_UserId_0
 */