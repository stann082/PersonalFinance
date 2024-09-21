using MongoDB.Driver;
using PersonalFinance.Application.Models;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;

namespace PersonalFinance.Application.Services;

public class StatementService(IMongoDatabase database) : IStatementService
{

    #region Public Methods

    public async Task<ITransaction[]> GetAsync(ISearchOptions? searchOptions = null)
    {
        IEnumerable<ITransaction> transactions = database.GetCollection<Transaction>("transactions").AsQueryable().ToArray<ITransaction>();
        if (searchOptions == null)
        {
            return transactions.ToArray();
        }

        return await Task.FromResult(transactions.ToArray());
    }

    public async Task SaveAsync(IParsedTransactionItem[] parsedItems, ISaveOptions saveOptions)
    {
        var transactionsCollection = database.GetCollection<Transaction>("transactions");

        int newTransactions = 0;
        int existingTransactions = 0;
        int updatedTransactions = 0;

        var transactions = parsedItems.Select(i => new Transaction(i)).ToArray();
        foreach (Transaction transaction in transactions)
        {
            var filter = Builders<Transaction>.Filter.And(
                Builders<Transaction>.Filter.Eq(f => f.Description, transaction.Description),
                Builders<Transaction>.Filter.Eq(f => f.Amount, transaction.Amount),
                Builders<Transaction>.Filter.Eq(f => f.Date, transaction.Date),
                Builders<Transaction>.Filter.Eq(f => f.Balance, transaction.Balance)
            );

            var existingTransaction = await transactionsCollection.Find(filter).FirstOrDefaultAsync();
            if (existingTransaction != null)
            {
                string consoleMessage;
                if (existingTransaction.IsRecurring != transaction.IsRecurring)
                {
                    var update = Builders<Transaction>.Update.Set(u => u.IsRecurring, transaction.IsRecurring);
                    await transactionsCollection.UpdateOneAsync(filter, update);

                    consoleMessage = $"Updated recurring status for: {existingTransaction}";
                    updatedTransactions++;
                }
                else
                {
                    consoleMessage = $"Transaction already exists with correct recurring status: {existingTransaction}";
                    existingTransactions++;
                }

                if (saveOptions.Verbose || updatedTransactions > 0)
                {
                    Console.WriteLine(consoleMessage);
                }

                continue;
            }

            newTransactions++;
            await transactionsCollection.InsertOneAsync(transaction);
        }

        Console.WriteLine($"Saved {newTransactions} new transactions. Updated {updatedTransactions} transactions. Skipped {existingTransactions} transactions that were already correct.");
    }

    #endregion

}
