namespace PersonalFinance.Domain.Models;

public interface IParsedTransactionItem : ITrainingModelItem
{
    string? Amount { get; }
    string? Balance { get; }
    string? Date { get; }
}
