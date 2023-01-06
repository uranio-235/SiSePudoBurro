using Coravel.Invocable;

using Domain.Abstract.DAL;
using Domain.Entitities;
using Domain.Events;

namespace Application.Balances.Jobs;
public class UpdateBalance : IInvocable, IInvocableWithPayload<PaymentReceived>
{
    private readonly IWriteDbContext _dbContext;
    public UpdateBalance(IWriteDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public PaymentReceived Payload { get; set; }

    public async Task Invoke()
    {
        var payment = _dbContext.Payment
            .Where(p => p.Id == Payload.PaymentId)
            .Select(p => new
            {
                p.CustomerId,
                p.Currency,
                p.FinalOutstandingAmount
            })
            .Single();

        using var transaction = await _dbContext.BeginTransactionAsync();

        var balance =
            _dbContext.Balance
            .Where(b => b.Currency == payment.Currency)
            .Where(b => b.CustomerId == payment.CustomerId)
            .SingleOrDefault() ?? _dbContext.Balance.Add(new Balance
            {
                CustomerId = payment.CustomerId,
                Currency = payment.Currency,
            }).Entity;

        await _dbContext.SaveChangesAsync();

        var outstandingBalance =
            _dbContext.Payment
                .Where(b => b.Currency == payment.Currency)
                .Where(b => b.CustomerId == payment.CustomerId)
                .Sum(p => p.FinalOutstandingAmount);

        // pagado!
        balance.AvailableBalance = outstandingBalance;
        _dbContext.Balance.Update(balance);
        await _dbContext.SaveChangesAsync();

        await transaction.CommitAsync();
    }
}
