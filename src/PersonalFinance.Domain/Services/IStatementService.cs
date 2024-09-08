using PersonalFinance.Domain.Models;

namespace PersonalFinance.Domain.Services;

public interface IStatementService
{
    Task<ITransaction[]> GetAsync(ISearchOptions? searchOptions = null);
    Task SaveAsync(IParsedTransactionItem[] parsedItems);
}
