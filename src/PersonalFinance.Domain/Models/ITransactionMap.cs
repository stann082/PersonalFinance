namespace PersonalFinance.Domain.Models;

public interface ITransactionMap
{
    string Date { get; }
    string? Description { get; }
    string Amount { get; }
}
