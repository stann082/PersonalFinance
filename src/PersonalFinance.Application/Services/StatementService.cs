using MongoDB.Driver;
using PersonalFinance.Application.Models;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;

namespace PersonalFinance.Application.Services;

public class StatementService(IMongoClient mongoClient) : IStatementService
{

    #region Variables

    private readonly IMongoDatabase _database = mongoClient.GetDatabase("financial_data");

    #endregion

    #region Public Methods

    public async Task<ITransaction[]> Get()
    {
        var transactions = _database.GetCollection<Transaction>("transactions").AsQueryable().ToArray<ITransaction>();
        return await Task.FromResult(transactions);
    }

    public async Task SaveAsync(IParsedTransactionItem[] parsedItems)
    {
        var transactionsCollection = _database.GetCollection<Transaction>("transactions");

        int newTransactions = 0;
        int existingTransactions = 0;

        var transactions = parsedItems.Select(i => new Transaction(i)).ToArray();
        foreach (Transaction transaction in transactions)
        {
            var filter = Builders<Transaction>.Filter.And(
                Builders<Transaction>.Filter.Eq(f => f.Description, transaction.Description),
                Builders<Transaction>.Filter.Eq(f => f.Amount, transaction.Amount),
                Builders<Transaction>.Filter.Eq(f => f.Date, transaction.Date)
            );

            var existingTransaction = await transactionsCollection.Find(filter).FirstOrDefaultAsync();
            if (existingTransaction != null)
            {
                existingTransactions++;
                continue;
            }

            newTransactions++;
            await transactionsCollection.InsertOneAsync(transaction);
        }

        Console.WriteLine($"Saved {newTransactions} new transactions. Skipped {existingTransactions} transactions that were already saved.");
    }

    #endregion

}
