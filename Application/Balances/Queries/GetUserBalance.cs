using FluentResults;

using FluentValidation;

using MediatR;

namespace Application.Balances.Queries;

public class GetUserBalance
{
    public record Query(Guid UserId) : IRequest<Result<Data>>;

    public class Validation : AbstractValidator<Query>
    {
        public Validation()
        {
            RuleFor(q => q.UserId)
                .Must(uid => uid != Guid.Empty)
                .WithMessage("ERROR");
        }
    }

    public class QueryHandler : IRequestHandler<Query, Result<Data>>
    {
        public QueryHandler()
        {

        }

        public async Task<Result<Data>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (request.UserId == Guid.Parse("00000000-0000-0000-0000-000000000001"))
                throw ValidationHelper.CreateValidationException(
                    "La Guid 01 es para probar un throw interno.");

            //return Result.Fail("Todo vien, pero esto no está implementado");
            return Result.Ok(new Data());
        }
    }

    public class Data
    {

    }

} // main class 
