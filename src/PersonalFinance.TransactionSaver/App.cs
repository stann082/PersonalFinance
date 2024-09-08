using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;
using PersonalFinance.Infrastructure;

public class App(
    IBankStatementParser statementParser,
    IStatementService statementService,
    ITransactionCategorizer transactionCategorizer)
{

    #region Public Methods

    public async Task<int> RunApp(IEnumerable<string> args)
    {
        string? oneDrivePath = Environment.GetEnvironmentVariable("OneDrive");
        if (oneDrivePath == null)
        {
            Console.WriteLine("OneDrive environment variable is not set.");
            return 1;
        }

        string financesPath = Path.Combine(oneDrivePath, "Documents", "personal", "finances");
        string categoryModelTrainingDataFilePath = Path.Combine(financesPath, "category-model-training-data.csv");
        if (!File.Exists(categoryModelTrainingDataFilePath))
        {
            Console.WriteLine($"File {categoryModelTrainingDataFilePath} not found");
            return 1;
        }

        string statementFilePath = Path.Combine(financesPath, "stmt-full.csv");
        if (!File.Exists(statementFilePath))
        {
            Console.WriteLine($"File {statementFilePath} not found");
            return 1;
        }

        ITransactionMap[] transactionMap = await statementParser.Parse(statementFilePath);
        Transaction[] transactions = transactionMap.Select(t => new Transaction(t)).ToArray();
        await transactionCategorizer.CategorizeAsync(categoryModelTrainingDataFilePath, transactions);
        await statementService.SaveAsync(transactions);
        return 0;
    }

    #endregion

}
