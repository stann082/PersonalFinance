namespace PersonalFinance.Domain.Models;

public interface ITransactionMapper
{
    string Date { get; }
    string? Description { get; }
    string Amount { get; }
}
