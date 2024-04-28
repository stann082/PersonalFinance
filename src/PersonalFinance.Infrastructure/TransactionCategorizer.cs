using Microsoft.ML;
using PersonalFinance.Domain.Models;
using PersonalFinance.Infrastructure.Models;

namespace PersonalFinance.Infrastructure;

public class TransactionCategorizer : ITransactionCategorizer
{

    public async Task CategorizeTransactions(string modelTrainingFilePath, IEnumerable<Transaction> transactions)
    {
        var mlContext = new MLContext();
        var trainingDataView = mlContext.Data.LoadFromTextFile<TransactionData>(modelTrainingFilePath,
            hasHeader: false,
            separatorChar: ',');
        
        Console.WriteLine("Load and prepare data");
        var loadedTransactions = mlContext.Data.CreateEnumerable<TransactionData>(trainingDataView, false).ToArray();
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
        var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(TransactionData.Description))
            .Append(mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "Category", outputColumnName: "Label"))
            .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
            .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
        var model = pipeline.Fit(trainingDataView);

        Console.WriteLine("Use the model for predictions");
        var predictor = await Task.FromResult(mlContext.Model.CreatePredictionEngine<TransactionData, TransactionPrediction>(model));
        foreach (Transaction transaction in transactions)
        {
            var prediction = predictor.Predict(new TransactionData { Description = transaction.Description });
            transaction.Category = prediction.PredictedCategory;
        }

        // TODO: Figure out the issue with batch-processing
        // IDataView transactionDataView = mlContext.Data.LoadFromEnumerable(transactions);
        // IDataView predictions = model.Transform(transactionDataView);
        // IEnumerable<TransactionPrediction> predictedCategories = mlContext.Data.CreateEnumerable<TransactionPrediction>(predictions, false);
        // Transaction[] transactionWithPredictions = transactions.Zip(predictedCategories, (transaction, prediction) =>
        // {
        //     transaction.Category = prediction.PredictedCategory;
        //     return transaction;
        // }).ToArray();
    }

}
