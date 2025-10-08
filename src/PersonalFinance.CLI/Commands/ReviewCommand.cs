using MongoDB.Driver;
using PersonalFinance.Application.Models;
using PersonalFinance.CLI.Options;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;
using PersonalFinance.Domain.Util;
using PersonalFinance.Infrastructure;

namespace PersonalFinance.CLI.Commands;

public class ReviewCommand(ReviewOptions options, IStatementService service, ITransactionCategorizer transactionCategorizer)
{
    
    #region Public Methods

    public async Task<int> Execute()
    {
        var pathProvider = new PathProvider();
        var hasValidPaths = pathProvider.Validate();
        if (!hasValidPaths.IsValid)
        {
            Console.WriteLine(hasValidPaths.Error);
            return 1;
        }
        
        IEnumerable<ITransaction> allTransactions = await service.GetAsync();
        DateTime startDate = DateTime.MinValue;
        if (!string.IsNullOrEmpty(options.StartDate))
        {
            startDate = DateTime.TryParse(options.StartDate, out var beforeDate) ? beforeDate : DateTime.MinValue;
        }
        
        var filteredTransactions = allTransactions.Where(t => t.Date >= startDate).ToArray();
        await transactionCategorizer.CategorizeAsync(pathProvider.CategoryModelTrainingDataFilePath, filteredTransactions);

        foreach (ITransaction transaction in filteredTransactions)
        {
            var filter = Builders<Transaction>.Filter.And(
                Builders<Transaction>.Filter.Eq(f => f.Description, transaction.Description),
                Builders<Transaction>.Filter.Eq(f => f.Amount, transaction.Amount),
                Builders<Transaction>.Filter.Eq(f => f.Date, transaction.Date),
                Builders<Transaction>.Filter.Eq(f => f.Balance, transaction.Balance)
            );

            var existingTransaction = await transactionsCollection.Find(filter).FirstOrDefaultAsync();
            Console.WriteLine($"{existingTransaction}; Recurring status: {existingTransaction.IsRecurring}");
            if (ConsoleHelper.ConfirmAction($"Would you like to update this transaction with {transaction.IsRecurring}?"))
            {
                var update = Builders<Transaction>.Update.Set(u => u.IsRecurring, transaction.IsRecurring);
                await transactionsCollection.UpdateOneAsync(filter, update);

                consoleMessage = $"Updated recurring status for: {existingTransaction}";
                updatedTransactions++;
            }
        }

        return await Task.FromResult(0);
    }
    
    #endregion
    
}