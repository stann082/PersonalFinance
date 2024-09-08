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

    public Transaction[] Get()
    {
        var transactionDocuments = _database.GetCollection<TransactionDocument>("transactions").AsQueryable().ToArray();
        return transactionDocuments.Select(CreateTransaction).ToArray();
    }

    public async Task SaveAsync(Transaction[] transactions)
    {
        var transactionsCollection = _database.GetCollection<TransactionDocument>("transactions");

        int newTransactions = 0;
        int existingTransactions = 0;

        foreach (Transaction transaction in transactions)
        {
            var filter = Builders<TransactionDocument>.Filter.And(
                Builders<TransactionDocument>.Filter.Eq(f => f.Description, transaction.Description),
                Builders<TransactionDocument>.Filter.Eq(f => f.Amount, transaction.Amount),
                Builders<TransactionDocument>.Filter.Eq(f => f.TransactionDate, transaction.Date)
            );

            var existingTransaction = await transactionsCollection.Find(filter).FirstOrDefaultAsync();
            if (existingTransaction != null)
            {
                existingTransactions++;
                continue;
            }

            TransactionDocument document = new TransactionDocument
            {
                TransactionId = Guid.NewGuid().ToString(),
                Amount = transaction.Amount,
                Category = transaction.Category,
                Description = transaction.Description,
                IsRecurring = transaction.IsRecurring,
                TransactionDate = transaction.Date
            };

            newTransactions++;
            await transactionsCollection.InsertOneAsync(document);
        }
        
        Console.WriteLine($"Saved {newTransactions} new transactions. Skipped {existingTransactions} transactions that were already saved.");
    }

    #endregion

    #region Helper Methods

    private static Transaction CreateTransaction(TransactionDocument document)
    {
        Transaction transaction = new Transaction(default);
        
        return transaction;
    }

    #endregion

}
