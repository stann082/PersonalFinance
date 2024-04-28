using Microsoft.ML.Data;

namespace MyApp;

public class TransactionPrediction
{
    [ColumnName("PredictedLabel")]
    public string PredictedCategory { get; set; }
}
