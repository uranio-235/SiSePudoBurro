using Application.Balances.Jobs;

using Coravel.Queuing.Interfaces;
using Domain.Abstract.DAL;
using Domain.Abstract.Services;
using Domain.SharedKernel;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payments.Commands;

public class QvapayPaymentGot
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
                .WithErrorCode(HttpStatusCode.NotFound.ToString())
                .WithMessage("No se recibió la id remote (el md5).");
        }
    }

    public class CommandHandler : IRequestHandler<Command, Result>
    {
        private readonly ILogger<QvapayPaymentGot> _logger;
        private readonly IWriteDbContext _dbContext;
        private readonly IQvapayClient _qvapay;
        private readonly IQueue _queue;

        public CommandHandler(
            ILogger<QvapayPaymentGot> logger,
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
            var transaction = await _dbContext.BeginTransactionAsync();

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

            payment.ExecutedAmount = invoice.Amount;
            
            _dbContext.Payment.Update(payment);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            _logger.LogInformation(
                "procesando pago {externalId} con un monto de {finalAmount}",
                payment.ExternalIdentification, payment.ExecutedAmount);

            // mándalo a actualizar el balance
            _queue.QueueInvocableWithPayload<UpdateCustomer, UpdateCustomer.Data>(new UpdateCustomer.Data
            {
                UserId = payment.UserId
            });

            return Result.Ok();
        }
    }

} // main class 