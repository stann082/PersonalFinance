using CsvHelper.Configuration.Attributes;
using PersonalFinance.Domain.Models;

namespace PersonalFinance.Infrastructure.Models;

public class ParsedTransactionItem : IParsedTransactionItem
{
    public string? Amount { get; set; }
    
    [Name("Running Bal.")]
    public string? Balance { get; set; }
    
    public string? Category { get; set; }
    
    public string? Date { get; set; }
    public string? Description { get; set; }
    
    public bool? IsRecurring { get; set; }
}
