using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;
using PersonalFinance.Domain.Util;
using Serilog;

namespace PersonalFinance.CLI.Commands;

public class RemoveCommand(IRemoveOptions options, IStatementService service)
{

    #region Public Methods

    public async Task<int> Execute()
    {
        if (options.DuplicateItems)
        {
            await RemoveDuplicates(options.ShowOnly);
            return 0;
        }
        
        await RemoveTransaction(options.Id);
        return await Task.FromResult(0);
    }

    #endregion

    #region Helper Methods

    private static bool ConfirmAction(string message)
    {
        Console.Write($"{message} (Y/N): ");
        string? input = Console.ReadLine();
        while (input == null || (!input.Equals("Y", StringComparison.OrdinalIgnoreCase) && !input.Equals("N", StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("Invalid input. Please enter 'Y' or 'N'.");
            input = Console.ReadLine();
        }
        
        return input.Equals("Y", StringComparison.OrdinalIgnoreCase);
    }

    private async Task RemoveDuplicates(bool showOnly)
    {
        IEnumerable<Transaction> allTransactions = await service.GetAsync();
        var duplicateTransactions = allTransactions
            .GroupBy(t => new { t.Amount, t.Date, t.Description, t.Balance })
            .Where(g => g.Count() > 1)
            .SelectMany(g => g)
            .ToList();
        
        foreach (var t in duplicateTransactions)
        {
            Console.WriteLine($"Amount: {t.Amount}, Date: {t.Date:yyyy-MM-dd}, Description: {t.Description}, Category: {t.Category}, Balance: {t.Balance}");
            if (showOnly)
            {
                continue;
            }
            
            if (ConfirmAction("Would you like to delete this transaction?"))
            {
                await RemoveTransaction(options.Id);
            }
        }
    }

    private async Task RemoveTransaction(string? id)
    {
        var deleteResult = await service.DeleteAsync(id);
        Log.Information("Deleted {DeleteResult} transaction(s)", deleteResult.DeletedCount);
    }

    #endregion

}