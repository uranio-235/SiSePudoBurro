using Application.Customers.Models;
using Application.Customers.Queries;

using Domain.Abstract.DAL;

using FluentResults;

using FluentValidation;

using MediatR;


namespace Application.Customers.Commands;

public class CreateCustomer
{
    public class Command : IRequest<Result<Customer>>
    {
        public long TelegramId { get; set; }
        public string Username { get; set; }
    }

    public class Validation : AbstractValidator<Command>
    {
        public Validation(IReadDbContext dbContext)
        {
            RuleFor(c => c.Username)
                .NotEmpty()
                .WithMessage("El username no es opcional papá.")

                .Must(u => !dbContext.Customer.Any(c => c.View.Username == u))
                .WithMessage("Ya existe un usuario con ese nombre");

            RuleFor(c => c.TelegramId)
                .Must(tid => tid != default)
                .WithMessage("El username no es opcional papá.");

            RuleFor(c => c.TelegramId.ToString())
                .Must(tid => !dbContext.Customer.Any(c => c.View.TelegramId == tid))
                .WithMessage("Ya existe un usuario con ese nombre");

        }
    }

    public class CommandHandler : IRequestHandler<Command, Result<Customer>>
    {
        private readonly IMediator _mediator;
        private readonly IWriteDbContext _dbContext;

        public CommandHandler(
            IMediator mediator,
            IWriteDbContext dbContext)
        {
            _mediator = mediator;
            _dbContext = dbContext;
        }

        public async Task<Result<Customer>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = _dbContext.Customer.Add(
            new Domain.Entitities.Customer
            {
                View = new Domain.Entitities.CustomerViewJson
                {
                    TelegramId = request.TelegramId.ToString(), // en la vista el id de telegram desnormalizado será string para no cargar el motor
                    Username = request.Username,
                    AvailableBalance = 0,
                    OutstandingBalance = 0,
                    TotalTransactions = 0
                },
                User = new Domain.Entitities.User
                {
                    TelegramId = request.TelegramId
                }
            }).Entity.UserId;

            _dbContext.SaveChanges();

            return await _mediator.Send(new GetCustomer.Query(userId));
        }
    }

} // main class 
