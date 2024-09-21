namespace PersonalFinance.Domain.Models;

public interface IParsedTransactionItem
{
    string Amount { get; }
    string Balance { get; }
    string Category { get; set; }
    string Date { get; }
    string? Description { get; }
    bool IsRecurring { get; set;  }
}
