using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;

namespace PersonalFinance.CLI.Commands;

public class RemoveCommand(IRemoveOptions options, IStatementService service)
{

    #region Public Methods

    public async Task<int> Execute()
    {
        IEnumerable<ITransaction> allTransactions = await service.GetAsync();
        var duplicateTransactions = allTransactions
            .GroupBy(t => new { t.Amount, t.Date, t.Description, t.Balance })
            .Where(g => g.Count() > 1)
            .SelectMany(g => g)
            .ToList();
        
        foreach (var t in duplicateTransactions)
        {
            Console.WriteLine($"Amount: {t.Amount}, Date: {t.Date:yyyy-MM-dd}, Description: {t.Description}, Category: {t.Category}, Balance: {t.Balance}");
        }
        return await Task.FromResult(0);
    }

    #endregion

    #region Helper Methods

    #endregion
    
}