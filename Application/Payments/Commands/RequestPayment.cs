using Application.Balances.Jobs;

using Domain.Abstract.DAL;
using Domain.Abstract.Services;
using Domain.Entitities;

using FluentResults;

using FluentValidation;

using MassTransit;

using MediatR;


using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace Application.Payments.Commands;

public class RequestPayment
{
    public record Command : IRequest<Result<Data>>
    {
        public Command()
        {

        }

        [JsonIgnore]
        public Guid UserId { get; set; }

        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class Validation : AbstractValidator<Command>
    {
        public Validation(IReadDbContext dbContext)
        {
            RuleFor(c => c.UserId)
                .MustBeValidUser(dbContext);

            RuleFor(c => c.Amount)
                .GreaterThan(0);
        }
    }

    public class CommandHandler : IRequestHandler<Command, Result<Data>>
    {
        private readonly IBus _bus;
        private readonly IQvapayClient _qvapayClient;
        private readonly IWriteDbContext _dbContext;

        public CommandHandler(
            IBus bus,
            IQvapayClient qvapayClient,
            IWriteDbContext dbContext)
        {
            _bus = bus;
            _qvapayClient = qvapayClient;
            _dbContext = dbContext;
        }

        public async Task<Result<Data>> Handle(Command request, CancellationToken cancellationToken)
        {
            var remoteId = string.Join("", MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(DateTime.UtcNow.Ticks.ToString())).Select(s => s.ToString("x2"))).ToUpperInvariant();

            var qvapayInvoice = await _qvapayClient
                .CreateInvoiceAsync(
                    amount: request.Amount,
                    description: request.Description,
                    remoteId: $"qvp_{remoteId.ToLowerInvariant()}");

            var payment = _dbContext.Payment.Add(new Payment
            {
                RequestedAmount = request.Amount,
                UserId = request.UserId
            }).Entity;

            await _dbContext.SaveChangesAsync();

            await _bus.Publish(
                new UpdateCustomer.Data
                {
                    UserId = payment.UserId
                });

            return new Data
            {
                PaymentId = payment.Id,
                Description = request.Description!,
                CreatedAt = payment.CreatedAt,
                UserId = payment.UserId,
                RemoteId = remoteId,
                URL = qvapayInvoice.Url
            }.ToResult();
        }
    }

    public class Data
    {
        public Guid PaymentId { get; internal set; }
        public string Description { get; internal set; } = string.Empty;
        public DateTime CreatedAt { get; internal set; }
        public Guid UserId { get; internal set; }
        public string RemoteId { get; internal set; } = string.Empty;
        public string URL { get; internal set; }
    }

} // main class 
