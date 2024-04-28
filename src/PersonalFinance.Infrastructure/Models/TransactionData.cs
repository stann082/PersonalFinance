using Microsoft.ML.Data;

namespace PersonalFinance.Infrastructure.Models;

public class TransactionData
{
    
    [LoadColumn(0)]
    public string? Description { get; set; }

    [LoadColumn(1)]
    public string Category { get; set; }
    
}
