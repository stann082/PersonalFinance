using MongoDB.Driver;
using PersonalFinance.Domain.Models;

namespace PersonalFinance.Domain.Services;

public interface IStatementService
{
    Task<Transaction[]> GetAsync(ISearchOptions? searchOptions);
    Task<Transaction[]> GetAsync(DateTime? startDate = null, DateTime? stopDate = null);
    Task<DeleteResult> DeleteAsync(string? id);
    Task SaveAsync(IParsedTransactionItem[] parsedItems, ISaveOptions saveOptions);
    Task UpdateAsync(Transaction transaction);
}
