﻿using Domain.Abstract.DAL;

using FluentResults;

using FluentValidation;

using MediatR;

namespace Application.Balances.Queries;

public class GetUserBalance
{
    public record Query(Guid UserId) : IRequest<Result<Data>>;

    public class Validation : AbstractValidator<Query>
    {
        public Validation(IReadDbContext dbContext)
        {
            RuleFor(q => q.UserId)
                .MustBeValidUser(dbContext);
        }
    }

    public class QueryHandler : IRequestHandler<Query, Result<Data>>
    {
        private readonly IReadDbContext _dbContext;

        public QueryHandler(IReadDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<Result<Data>> Handle(Query request, CancellationToken cancellationToken)
            => Task.FromResult(_dbContext.Customer
                .Where(c => c.UserId == request.UserId)
                .Select(c => new Data
                {
                    AvailableBalance = c.View.AvailableBalance,
                    OutstandingBalance = c.View.OutstandingBalance,
                })
                .Single()
            .ToResult());

    }

    public class Data
    {
        public decimal AvailableBalance { get; set; }
        public decimal OutstandingBalance { get; internal set; }
    }

} // main class 

/*
SELECT 
CAST(JSON_VALUE([c].[View],'$.AvailableBalance') AS decimal(18,2)) AS [AvailableBalance],
CAST(JSON_VALUE([c].[View],'$.OutstandingBalance') AS decimal(18,2)) AS [OutstandingBalance]
FROM [Customer] AS [c]
 */