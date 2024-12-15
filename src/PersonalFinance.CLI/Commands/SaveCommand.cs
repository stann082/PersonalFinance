using System.Text.RegularExpressions;
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
            Console.WriteLine("OneDrive environment variable is not found.");
            return 1;
        }

        string financesPath = Path.Combine(oneDrivePath, "Documents", "personal", "finances");
        string categoryModelTrainingDataFilePath = Path.Combine(financesPath, "category-model-training-data.csv");
        if (!File.Exists(categoryModelTrainingDataFilePath))
        {
            Console.WriteLine($"File {categoryModelTrainingDataFilePath} not found");
            return 1;
        }

        string? userProfilePath = Environment.GetEnvironmentVariable("USERPROFILE");
        if (userProfilePath == null)
        {
            Console.WriteLine($"USERPROFILE environment variable not found.");
            return 1;
        }
        
        string downloadsPath = Path.Combine(userProfilePath, "Downloads");
        string downloadedStatementFilePath = Path.Combine(downloadsPath, "stmt.csv");
        if (!File.Exists(downloadedStatementFilePath))
        {
            Console.WriteLine($"File {downloadedStatementFilePath} not found.");
            return 1;
        }

        string date = ParseStatementDate(downloadedStatementFilePath);
        string statementFilePath = Path.Combine(financesPath, $"bofa-stmt-{date}");
        File.Copy(downloadedStatementFilePath, statementFilePath, true);
        if (!File.Exists(statementFilePath))
        {
            Console.WriteLine($"File {statementFilePath} not found.");
            return 1;
        }

        RemoveUnwantedLines(statementFilePath, 6);
        
        IParsedTransactionItem[] transactions = await statementParser.Parse(statementFilePath);
        await transactionCategorizer.CategorizeAsync(categoryModelTrainingDataFilePath, transactions);
        await statementService.SaveAsync(transactions, options);

        if (File.Exists(downloadedStatementFilePath))
        {
            File.Delete(downloadedStatementFilePath);
        }
        
        return 0;
    }
    
    private static string ParseStatementDate(string filePath)
    {
        string date = DateTime.UtcNow.ToShortDateString();
        const string datePattern = @"\b\d{1,2}/\d{1,2}/\d{4}\b";
        string[] lines = File.ReadAllLines(filePath);
        foreach (var line in lines)
        {
            if (!line.Contains("Ending balance"))
            {
                continue;
            }

            Match match = Regex.Match(line, datePattern);
            if (!match.Success)
            {
                continue;
            }
            
            date = match.Value;
            break;
        }

        return date.Replace('/', '-');
    }
    
    private static void RemoveUnwantedLines(string filePath, int linesToRemove)
    {
        string[] lines = File.ReadAllLines(filePath);
        File.WriteAllLines(filePath, lines.Skip(linesToRemove).ToArray());
        
        lines = File.ReadAllLines(filePath);
        var filteredLines = lines.Where(line => 
            !line.Contains("Beginning balance as of") && 
            !line.Contains("Total credits") && 
            !line.Contains("Total debits") && 
            !line.Contains("Ending balance as of")
        ).ToArray();

        File.WriteAllLines(filePath, filteredLines);
    }
    
}
