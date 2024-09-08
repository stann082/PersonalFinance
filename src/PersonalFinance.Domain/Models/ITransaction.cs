using PersonalFinance.Domain.Misc;

namespace PersonalFinance.Domain.Models;

public interface ITransaction
{
    
    decimal Amount { get; }
    string? Category { get; }
    DateTime Date { get; }
    string? Description { get; }
    bool? IsRecurring { get; }
    string? TransactionId { get; }
    TransactionType TransactionType { get; }
    
}
