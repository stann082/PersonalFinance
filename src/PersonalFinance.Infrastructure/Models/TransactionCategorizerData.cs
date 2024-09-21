using Microsoft.ML.Data;

namespace PersonalFinance.Infrastructure.Models;

public class TransactionCategorizerData
{
    
    [LoadColumn(0)]
    public string? Description { get; set; }

    [LoadColumn(1)]
    public string? Category { get; set; }
    
    [LoadColumn(2)]
    public bool IsRecurring { get; set; }
    
}
