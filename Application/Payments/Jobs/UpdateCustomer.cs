using Coravel.Invocable;

using Domain.Abstract.DAL;
using Domain.SharedKernel;

namespace Application.Balances.Jobs;
public class UpdateCustomer : IInvocable, IInvocableWithPayload<UpdateCustomer.Data>
{
    private readonly IWriteDbContext _dbContext;
    public UpdateCustomer(IWriteDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public sealed class Data
    {
        public Guid UserId { get; set; }
        public string? Username { get; set; }
    }

    public Data Payload { get; set; }

    public async Task Invoke()
    {
        using var transaction = await _dbContext.BeginTransactionAsync();

        var customer = _dbContext.Customer
            .SingleOrDefault(c => c.UserId == Payload.UserId);

        if (customer is null)
            return;

        var view = customer.View ?? new();

        // si cambió el username lo cambio aquí
        view.Username = Payload.Username ?? view.Username;

        var query = _dbContext.Payment
            .Where(p => p.UserId == Payload.UserId);

        view.OutstandingBalance = query
            .Where(p => p.Status != PaymentStatus.Completed)
            .Sum(p => p.RequestedAmount);

        view.AvailableBalance = query
            .Where(p => p.Status == PaymentStatus.Completed)
            .Sum(p => p.ExecutedAmount);

        view.TotalTransactions =
            _dbContext.Payment.Count();

        customer.View = view;
        _dbContext.Customer.Update(customer);

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
