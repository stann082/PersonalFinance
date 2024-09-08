using PersonalFinance.Domain.Models;

namespace PersonalFinance.Domain.Services;

public interface IStatementService
{
    Task<ITransaction[]> Get();
    Task SaveAsync(IParsedTransactionItem[] parsedItems);
}
