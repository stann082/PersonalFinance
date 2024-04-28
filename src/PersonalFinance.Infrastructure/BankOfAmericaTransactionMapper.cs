using PersonalFinance.Domain.Models;

namespace PersonalFinance.Infrastructure;

public class BankOfAmericaTransactionMapper : ITransactionMapper
{
    public string Date { get; set; }
    public string? Description { get; set; }
    public string Amount { get; set; }
}
