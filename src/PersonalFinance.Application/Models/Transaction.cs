using MongoDB.Bson;
using PersonalFinance.Domain.Misc;
using PersonalFinance.Domain.Models;

namespace PersonalFinance.Application.Models;

public class Transaction : ITransaction
{

    #region Constructors

    public Transaction(IParsedTransactionItem item)
    {
        InitializeProperties(item);
    }

    #endregion
    
    #region Properties

    public ObjectId Id { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public string? Category { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public bool? IsRecurring { get; set; }
    public string? TransactionId { get; set; }
    public TransactionType TransactionType { get; set; }

    #endregion
    
    #region Overridden Methods

    public override string ToString()
    {
        return $"{Date.ToShortDateString()}: {Description}";
    }

    #endregion
    
    #region Helper Methods

    private void InitializeProperties(IParsedTransactionItem item)
    {
        TransactionId = Guid.NewGuid().ToString();
        Amount = decimal.TryParse(item.Amount, out var amountResult) ? amountResult : 0;
        Balance = decimal.TryParse(item.Balance, out var balanceResult) ? balanceResult : 0;
        Category = item.Category;
        Date = DateTime.TryParse(item.Date, out var dateResult) ? dateResult : DateTime.MinValue;
        Description = item.Description;
        IsRecurring = item.IsRecurring;
        TransactionType = Amount < 0 ? TransactionType.Debit : TransactionType.Credit;
    }

    #endregion

}
