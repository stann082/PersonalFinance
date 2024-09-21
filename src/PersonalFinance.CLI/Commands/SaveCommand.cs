using PersonalFinance.CLI.Options;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;
using PersonalFinance.Infrastructure;

namespace PersonalFinance.CLI.Commands;

public class SaveCommand(SaveOptions options, 
    IBankStatementParser statementParser,
    IStatementService statementService,
    ITransactionCategorizer transactionCategorizer)
{

    public async Task<int> Execute()
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

        IParsedTransactionItem[] transactions = await statementParser.Parse(statementFilePath);
        await transactionCategorizer.CategorizeAsync(categoryModelTrainingDataFilePath, transactions);
        await statementService.SaveAsync(transactions);
        return 0;
    }
    
}
