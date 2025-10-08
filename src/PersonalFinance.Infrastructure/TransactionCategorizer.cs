using Microsoft.ML;
using Microsoft.ML.Data;
using PersonalFinance.Domain.Models;
using PersonalFinance.Infrastructure.Models;

namespace PersonalFinance.Infrastructure;

public class TransactionCategorizer : ITransactionCategorizer
{

    #region Public Methods

    public async Task CategorizeAsync(string modelTrainingFilePath, IEnumerable<ITrainingModelItem> transactions)
    {
        var mlContext = new MLContext();
        var trainingDataView = mlContext.Data.LoadFromTextFile<TransactionCategorizerData>(modelTrainingFilePath,
            hasHeader: false,
            separatorChar: ',');

        Console.WriteLine("Load and prepare data");
        var loadedTransactions = mlContext.Data.CreateEnumerable<TransactionCategorizerData>(trainingDataView, false).ToArray();
        if (loadedTransactions.Select(t => t.Description).FirstOrDefault() == null)
        {
            Console.WriteLine("Description must have a value");
            return;
        }

        if (loadedTransactions.Select(t => t.Category).FirstOrDefault() == null)
        {
            Console.WriteLine("Category must have a value");
            return;
        }

        Console.WriteLine("Build and train the model");
        var categoryPipeline = GetCategoryPipeline(mlContext);
        var categoryModel = categoryPipeline.Fit(trainingDataView);

        var recurringPipeline = GetRecurringPipeline(mlContext);
        var recurringModel = recurringPipeline.Fit(trainingDataView);

        Console.WriteLine("Use the model for predictions");
        var categoryPredictor = await Task.FromResult(mlContext.Model.CreatePredictionEngine<TransactionCategorizerData, CategoryPrediction>(categoryModel));
        var recurringPredictor = await Task.FromResult(mlContext.Model.CreatePredictionEngine<TransactionCategorizerData, RecurringPrediction>(recurringModel));
        foreach (ITrainingModelItem transaction in transactions)
        {
            var transactionData = new TransactionCategorizerData { Description = transaction.Description };
            var categoryPrediction = categoryPredictor.Predict(transactionData);
            var recurringPrediction = recurringPredictor.Predict(transactionData);
            transaction.Category = categoryPrediction.PredictedCategory;
            transaction.IsRecurring = recurringPrediction.PredictedRecurring;
        }
    }

    #endregion

    #region Helper Methods

    private static IEstimator<ITransformer> GetCategoryPipeline(MLContext mlContext)
    {
        var categoryPipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(TransactionCategorizerData.Description))
            .Append(mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "Category", outputColumnName: "Label"))
            .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
            .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
        return categoryPipeline;
    }

    private IEstimator<ITransformer> GetRecurringPipeline(MLContext mlContext)
    {
        var recurringPipeline = mlContext.Transforms.Text.FeaturizeText("Description_tf", "Description")
            .Append(mlContext.Transforms.Concatenate("Features", "Description_tf"))
            .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression("IsRecurring"));
        return recurringPipeline;
    }

    #endregion

    #region Helper Class

    private class CategoryPrediction
    {
        [ColumnName("PredictedLabel")]
        public string? PredictedCategory { get; set; }
    }

    private class RecurringPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool PredictedRecurring { get; set; }
    }

    #endregion

}
