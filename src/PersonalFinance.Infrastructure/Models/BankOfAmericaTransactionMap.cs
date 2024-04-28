using PersonalFinance.Domain.Models;

namespace PersonalFinance.Infrastructure.Models;

public class BankOfAmericaTransactionMap : ITransactionMap
{
    public string Date { get; set; }
    public string? Description { get; set; }
    public string Amount { get; set; }
}
