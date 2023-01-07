using Domain.Abstract.DAL;

using FluentResults;

using MediatR;

namespace Application.Customers.Queries;

public class ListAllCustomers
{
    public record Query : IRequest<Result<List<Data>>>;

    public class QueryHandler : IRequestHandler<Query, Result<List<Data>>>
    {
        private readonly IReadDbContext _dbContext;

        public QueryHandler(IReadDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<Result<List<Data>>> Handle(Query request, CancellationToken cancellationToken)
            => Task.FromResult(
                Result.Ok(
                    _dbContext.Customer
                        .Select(c => new Data
                        {
                            TelergamId = c.View.TelegramId,
                            UserId = c.UserId,
                            Username = c.View.Username
                        })
                .ToList()));
    }


    public class Data
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string TelergamId { get; set; }
    }

} // main class 

/*
SELECT
CAST(JSON_VALUE([c].[View],'$.TelegramId') AS nvarchar(max)) AS [TelergamId],[c].[UserId],
CAST(JSON_VALUE([c].[View],'$.Username') AS nvarchar(max)) AS [Username]
FROM [Customer] AS [c]
 */