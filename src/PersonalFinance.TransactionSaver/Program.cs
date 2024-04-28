using Microsoft.ML;
using PersonalFinance.Domain.Models;
using PersonalFinance.Infrastructure;

namespace MyApp;

public static class Program
{
    public static async Task Main(string[] args)
    {
        string? oneDrivePath = Environment.GetEnvironmentVariable("OneDrive");
        if (oneDrivePath == null)
        {
            Console.WriteLine("OneDrive environment variable is not set.");
            return;
        }

        string financesPath = Path.Combine(oneDrivePath, "Documents", "personal", "finances");
        string transactionsMapFilePath = Path.Combine(financesPath, "transactions-map.csv");
        if (!File.Exists(transactionsMapFilePath))
        {
            Console.WriteLine($"File {transactionsMapFilePath} not found");
            return;
        }

        string statementFilePath = Path.Combine(financesPath, "stmt-full.csv");
        if (!File.Exists(statementFilePath))
        {
            Console.WriteLine($"File {statementFilePath} not found");
            return;
        }

        Console.WriteLine("Load and prepare data");
        var mlContext = new MLContext();
        IDataView trainingDataView = mlContext.Data.LoadFromTextFile<TransactionData>(transactionsMapFilePath,
            hasHeader: false,
            separatorChar: ',');
        
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
        ITransactionMapper[] transactionMapper = await BankOfAmericaStatementParser.Parse(statementFilePath);
        Transaction[] transactions = transactionMapper.Select(t => new Transaction(t)).ToArray();
        var predictor = mlContext.Model.CreatePredictionEngine<TransactionData, TransactionPrediction>(model);
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
