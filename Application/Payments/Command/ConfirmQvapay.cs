using Application.Balances.Jobs;

using Coravel.Queuing.Interfaces;

using Domain.Abstract.DAL;
using Domain.Abstract.Services;
using Domain.Events;
using Domain.SharedKernel;

using FluentResults;

using FluentValidation;

using MediatR;

using Microsoft.Extensions.Logging;

using System.Net;

namespace Application.Payments.Command;

public class ConfirmQvapay
{
    public record Command(string QvapayId, string RemoteId) : IRequest<Result>;

    public class Validation : AbstractValidator<Command>
    {
        public Validation(IReadDbContext dbContext)
        {
            RuleFor(t => t.QvapayId)
                .NotEmpty()
                .OverridePropertyName("id")
                .WithMessage("No se recibió la id de qvapay.");

            RuleFor(t => t.RemoteId)
                .NotEmpty()
                .OverridePropertyName("remote_id")
                .WithMessage("No se recibió la id remote (el md5).");
        }
    }

    public class CommandHandler : IRequestHandler<Command, Result>
    {
        private readonly ILogger<ConfirmQvapay> _logger;
        private readonly IWriteDbContext _dbContext;
        private readonly IQvapayClient _qvapay;
        private readonly IQueue _queue;

        public CommandHandler(
            ILogger<ConfirmQvapay> logger,
            IWriteDbContext dbContext,
            IQvapayClient qvapay,
            IQueue queue)
        {
            _logger = logger;
            _dbContext = dbContext;
            _qvapay = qvapay;
            _queue = queue;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var payment = _dbContext.Payment
                .Where(p => p.Status == PaymentStatus.Requested)
                .SingleOrDefault(p => p.ExternalIdentification == request.RemoteId);

            if (payment is { Status: not PaymentStatus.Requested })
                throw ValidationHelper.CreateValidationException(
                    "no hay ningún pago con esa id que esté esperando confirmación",
                    HttpStatusCode.NotFound);

            var invoice = await _qvapay.GetInvoiceAsync(request.QvapayId);

            _logger.LogInformation($"qvpay informa de pago recibido {request.RemoteId} del pago {payment.Id}");

            // pending/paid/cancelled
            payment.Status = invoice.Status switch
            {
                "pending" => PaymentStatus.Requested,
                "paid" => PaymentStatus.Completed,
                "cancelled" => PaymentStatus.Canceled,
                _ => PaymentStatus.Unknown
            };

            _dbContext.Payment.Update(payment);
            await _dbContext.SaveChangesAsync();

            if (payment is { Status: not PaymentStatus.Completed })
            {
                _logger.LogCritical(
                    $"algo muy jodido pasó, la factura {request.RemoteId} vino con estado «{invoice.Status}»");

                throw ValidationHelper.CreateValidationException(
                    "no hay ningún pago con esa id que esté esperando confirmación",
                    HttpStatusCode.BadRequest);
            }

            payment.FinalOutstandingAmount = invoice.Amount;
            _dbContext.Payment.Update(payment);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "procesando pago {externalId} con un monto de {finalAmount}",
                payment.ExternalIdentification, payment.FinalOutstandingAmount);

            // mándalo a actualizar el balance
            _queue.QueueInvocableWithPayload<UpdateBalance, PaymentReceived>(new PaymentReceived
            {
                Amount = invoice.Amount,
                PaymentId = payment.Id
            });

            return Result.Ok();
        }
    }

} // main class 

