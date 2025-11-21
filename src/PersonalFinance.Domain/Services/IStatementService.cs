using MongoDB.Driver;
using PersonalFinance.Domain.Models;

namespace PersonalFinance.Domain.Services;

public interface IStatementService
{
    Task<Transaction[]> GetAsync(ISearchOptions? searchOptions = null);
    Task<DeleteResult> DeleteAsync(string? id);
    Task SaveAsync(IParsedTransactionItem[] parsedItems, ISaveOptions saveOptions);
}
