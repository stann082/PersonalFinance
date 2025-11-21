using PersonalFinance.Domain.Misc;

namespace PersonalFinance.Domain.Models;

public interface ITransaction : ITrainingModelItem
{
    
    decimal Amount { get; }
    decimal Balance { get; }
    DateTime Date { get; }
    bool IsDeposit { get; }
    string? TransactionId { get; }
    TransactionType TransactionType { get; }
    
}
