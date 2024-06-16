using Microsoft.ML.Data;

namespace PersonalFinance.Infrastructure.Models;

public class TransactionPrediction
{
    [ColumnName("PredictedLabel")]
    public string PredictedCategory { get; set; }
    
    public bool PredictedRecurring { get; set; }
}
