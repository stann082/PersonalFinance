using System.Text.RegularExpressions;
using PersonalFinance.CLI.Options;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;
using PersonalFinance.Domain.Util;
using PersonalFinance.Infrastructure;

namespace PersonalFinance.CLI.Commands;

public class SaveCommand(SaveOptions options, IBankStatementParser statementParser, IStatementService statementService, ITransactionCategorizer transactionCategorizer)
{

    public async Task<int> Execute()
    {
        var pathProvider = new PathProvider();
        var hasValidPaths = pathProvider.Validate();
        if (!hasValidPaths.IsValid)
        {
            Console.WriteLine(hasValidPaths.Error);
            return 1;
        }
        
        string date = ParseStatementDate(pathProvider.DownloadedStatementFilePath);
        string statementFilePath = Path.Combine(pathProvider.FinancesPath, $"bofa-stmt-{date}.csv");
        File.Copy(pathProvider.DownloadedStatementFilePath, statementFilePath, true);
        if (!File.Exists(statementFilePath))
        {
            Console.WriteLine($"File {statementFilePath} not found.");
            return 1;
        }

        RemoveUnwantedLines(statementFilePath, 6);
        
        IParsedTransactionItem[] transactions = await statementParser.Parse(statementFilePath);
        await transactionCategorizer.CategorizeAsync(pathProvider.CategoryModelTrainingDataFilePath, transactions);
        await statementService.SaveAsync(transactions, options);

        if (File.Exists(pathProvider.DownloadedStatementFilePath))
        {
            File.Delete(pathProvider.DownloadedStatementFilePath);
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
