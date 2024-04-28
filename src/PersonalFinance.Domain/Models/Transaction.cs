using PersonalFinance.Domain.Misc;

namespace PersonalFinance.Domain.Models;

public class Transaction
{

    public Transaction(ITransactionMapper transactionMapper)
    {
        InitializeProperties(transactionMapper);
    }

    #region Properties

    public decimal Amount { get; private set; }
    public string? Bank { get; private set; }
    public string? Category { get; set; }
    public DateTime Date { get; private set; }
    public string? Description { get; private set; }
    public TransactionType TransactionType { get; private set; }

    #endregion

    #region Overridden Methods

    public override string? ToString()
    {
        return Description;
    }

    #endregion
    
    #region Helper Methods

    private void InitializeProperties(ITransactionMapper transactionMapper)
    {
        Amount = decimal.TryParse(transactionMapper.Amount, out var amountResult) ? amountResult : 0;
        Bank = Constants.BofA;
        Date = DateTime.TryParse(transactionMapper.Date, out var dateResult) ? dateResult : DateTime.MinValue;
        Description = transactionMapper.Description;
        TransactionType = Amount < 0 ? TransactionType.Debit : TransactionType.Credit;
    }

    #endregion

}
