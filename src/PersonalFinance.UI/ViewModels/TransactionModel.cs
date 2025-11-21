using PersonalFinance.Domain.Misc;
using PersonalFinance.Domain.Models;

namespace PersonalFinance.UI.ViewModels;

public class TransactionModel : ITransaction
{

    #region Properties

    public decimal Amount { get; set; }
    
    public decimal Balance { get; set; }
    
    public string? Category { get; set; }
    
    public bool IsRecurring { get; set; }
    
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    
    public string? TransactionId { get; set; }
    public TransactionType TransactionType { get; set; }

    #endregion
    
}
