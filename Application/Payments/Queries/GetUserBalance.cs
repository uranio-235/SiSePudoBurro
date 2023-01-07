using Domain.Abstract.DAL;
using Domain.SharedKernel;

using FluentResults;

using FluentValidation;

using MediatR;

namespace Application.Balances.Queries;

public class GetUserBalance
{
    public record Query(Guid UserId) : IRequest<Result<IEnumerable<Data>>>;

    public class Validation : AbstractValidator<Query>
    {
        public Validation(IReadDbContext dbContext)
        {
            RuleFor(q => q.UserId)
                .Must(uid => dbContext.Customer.Any(c => c.UserId == uid))
                .WithMessage("ERROR");
        }
    }

    public class QueryHandler : IRequestHandler<Query, Result<IEnumerable<Data>>>
    {
        public QueryHandler()
        {

        }

        public async Task<Result<IEnumerable<Data>>> Handle(Query request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class Data
    {
        public LocalCurrency Currency { get; set; }
        public decimal Amount { get; set; }
    }

} // main class 
