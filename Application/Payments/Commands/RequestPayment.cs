using Application.Balances.Jobs;

using Coravel.Queuing.Interfaces;

using Domain.Abstract.DAL;
using Domain.Abstract.Services;
using Domain.Entitities;
using Domain.SharedKernel;

using FluentResults;

using FluentValidation;

using MediatR;

using System.Security.Cryptography;
using System.Text;

namespace Application.Payments.Commands;

public class RequestPayment
{
    public class Command : IRequest<Result<Data>>
    {
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public LocalCurrency Currency { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class Validation : AbstractValidator<Command>
    {
        public Validation(IReadDbContext dbContext)
        {
            RuleFor(c => c.UserId)
                .MustBeValidUser(dbContext);
        }
    }

    public class CommandHandler : IRequestHandler<Command, Result<Data>>
    {
        private readonly IQueue _queue;
        private readonly IQvapayClient _qvapayClient;
        private readonly IWriteDbContext _dbContext;

        public CommandHandler(
            IQueue queue,
            IQvapayClient qvapayClient,
            IWriteDbContext dbContext)
        {
            _queue = queue;
            _qvapayClient = qvapayClient;
            _dbContext = dbContext;
        }

        public async Task<Result<Data>> Handle(Command request, CancellationToken cancellationToken)
        {
            var remoteId = string.Join( "", MD5.Create().ComputeHash( Encoding.ASCII.GetBytes( DateTime.UtcNow.Ticks.ToString())) .Select(s => s.ToString("x2"))).ToUpperInvariant();

            var qvapayInvoice = _qvapayClient
                .CreateInvoiceAsync(
                    amount: request.Amount,
                    description: request.Description,
                    remoteId: $"qvp_{remoteId.ToLowerInvariant()}");

            var payment = _dbContext.Payment.Add(new Payment
            {
                Currency= request.Currency,
                RequestedAmount = request.Amount,
                UserId = request.UserId
            }).Entity;

            await _dbContext.SaveChangesAsync();

            _queue.QueueInvocableWithPayload<UpdateCustomer, UpdateCustomer.Data>(new UpdateCustomer.Data
            {
                UserId = payment.UserId
            });

            return new Data
            {
                PaymentId = payment.Id,
                Currency = request.Currency,
                Description = request.Description!,
                CreatedAt = payment.CreatedAt,
                UserId = payment.UserId,
                RemoteId = remoteId
            }.ToResult();
        }
    }

    public class Data
    {
        public Guid PaymentId { get; internal set; }
        public LocalCurrency Currency { get; internal set; }
        public string Description { get; internal set; } = string.Empty;
        public DateTime CreatedAt { get; internal set; }
        public Guid UserId { get; internal set; }
        public string RemoteId { get; internal set; } = string.Empty;
    }

} // main class 
