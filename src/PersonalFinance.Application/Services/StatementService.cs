using MongoDB.Bson;
using MongoDB.Driver;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;

namespace PersonalFinance.Application.Services;

public class StatementService(IMongoDatabase database) : IStatementService
{
    
    #region Public Methods

    public async Task<Transaction[]> GetAsync(ISearchOptions? options = null)
    {
        return await GetAsync(options?.StartDate, options?.StopDate);
    }

    public async Task<Transaction[]> GetAsync(DateTime? startDate = null, DateTime? stopDate = null)
    {
        var collection = GetTransactionsCollection();
        var b = Builders<Transaction>.Filter;

        var filters = new List<FilterDefinition<Transaction>>();

        if (startDate is { } start)
        {
            filters.Add(b.Gte(t => t.Date, start));
        }

        if (stopDate is { } stop)
        {
            filters.Add(b.Lte(t => t.Date, stop));
        }

        var finalFilter = filters.Count > 0 ? b.And(filters) : FilterDefinition<Transaction>.Empty;
        var result = await collection
            .Find(finalFilter)
            .SortBy(t => t.Date)
            .ToListAsync();
        return result.ToArray();
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        var collection = GetTransactionsCollection();
        var filter = Builders<Transaction>.Filter.Eq(t => t.Id, transaction.Id);
        var update = Builders<Transaction>.Update
            .Set(t => t.IsRecurring, transaction.IsRecurring)
            .Set(t => t.Description, transaction.Description)
            .Set(t => t.Category, transaction.Category);
        await collection.UpdateOneAsync(filter, update);
    }

    public async Task<DeleteResult> DeleteAsync(string? id)
    {
        var transactionsCollection = GetTransactionsCollection();
        var filter = Builders<Transaction>.Filter.Eq(x => x.TransactionId, id);
        return await transactionsCollection.DeleteOneAsync(filter);
    }

    public async Task SaveAsync(IParsedTransactionItem[] parsedItems, ISaveOptions saveOptions)
    {
        var transactionsCollection = GetTransactionsCollection();

        int newTransactions = 0;
        int existingTransactions = 0;
        int updatedTransactions = 0;
        int reconciledTransactions = 0;

        Transaction[] transactions = parsedItems.Select(i => new Transaction(i)).ToArray();
        foreach (Transaction transaction in transactions)
        {
            Transaction? existingTransaction = await FindExistingAsync(transactionsCollection, transaction);

            // A match found by content rather than by id is a transaction we already imported
            // from CSV and are now seeing again from SimpleFIN. Stamp the id onto it so later
            // syncs match directly instead of inserting a duplicate.
            if (existingTransaction is { ExternalId: null } && transaction.ExternalId != null)
            {
                await transactionsCollection.UpdateOneAsync(
                    Builders<Transaction>.Filter.Eq(f => f.Id, existingTransaction.Id),
                    Builders<Transaction>.Update.Set(u => u.ExternalId, transaction.ExternalId));

                existingTransaction.ExternalId = transaction.ExternalId;
                reconciledTransactions++;

                if (saveOptions.Verbose)
                {
                    Console.WriteLine($"Linked existing transaction to SimpleFIN id {transaction.ExternalId}: {existingTransaction}");
                }
            }

            if (existingTransaction != null)
            {
                string consoleMessage;
                if (transaction.IsRecurring == true && existingTransaction.IsRecurring != true)
                {
                    var update = Builders<Transaction>.Update.Set(u => u.IsRecurring, true);
                    await transactionsCollection.UpdateOneAsync(
                        Builders<Transaction>.Filter.Eq(f => f.Id, existingTransaction.Id), update);

                    consoleMessage = $"Updated recurring status for: {existingTransaction}";
                    updatedTransactions++;
                }
                else
                {
                    consoleMessage = $"Transaction already exists with correct recurring status: {existingTransaction}";
                    existingTransactions++;
                }

                if ((saveOptions.Verbose || updatedTransactions > 0) && !string.IsNullOrWhiteSpace(consoleMessage))
                {
                    Console.WriteLine(consoleMessage);
                }

                continue;
            }

            newTransactions++;
            await transactionsCollection.InsertOneAsync(transaction);
        }

        string reconciledMessage = reconciledTransactions > 0
            ? $" Linked {reconciledTransactions} previously imported transactions to their source id."
            : string.Empty;
        Console.WriteLine($"Saved {newTransactions} new transactions. Updated {updatedTransactions} transactions. Skipped {existingTransactions} transactions that were already correct.{reconciledMessage}");
    }

    public async Task EnsureIndexesAsync()
    {
        var collection = GetTransactionsCollection();

        // A sparse index is not enough here: the driver writes an explicit null for
        // CSV-imported transactions, and sparse only skips documents where the field is
        // absent, so every CSV row would collide on null. Restricting the index to string
        // values leaves them out entirely.
        var options = new CreateIndexOptions<Transaction>
        {
            Name = "external_id_unique",
            Unique = true,
            PartialFilterExpression = Builders<Transaction>.Filter.Type(t => t.ExternalId, BsonType.String)
        };

        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<Transaction>(Builders<Transaction>.IndexKeys.Ascending(t => t.ExternalId), options));
    }

    #endregion

    #region Helper Methods

    public IMongoCollection<Transaction> GetTransactionsCollection()
    {
        return database.GetCollection<Transaction>("transactions");
    }

    /// <summary>
    /// Locates the stored copy of a transaction, preferring the source's own id when there is
    /// one and falling back to content matching otherwise.
    /// </summary>
    private static async Task<Transaction?> FindExistingAsync(
        IMongoCollection<Transaction> collection, Transaction transaction)
    {
        var b = Builders<Transaction>.Filter;

        if (transaction.ExternalId != null)
        {
            var byExternalId = await collection
                .Find(b.Eq(f => f.ExternalId, transaction.ExternalId))
                .FirstOrDefaultAsync();
            if (byExternalId != null)
            {
                return byExternalId;
            }
        }

        // Balance participates in the match only for CSV imports, which are the only source
        // that supplies a running balance.
        var contentFilters = new List<FilterDefinition<Transaction>>
        {
            b.Eq(f => f.Description, transaction.Description),
            b.Eq(f => f.Amount, transaction.Amount),
            b.Eq(f => f.Date, transaction.Date)
        };

        if (transaction.ExternalId == null)
        {
            contentFilters.Add(b.Eq(f => f.Balance, transaction.Balance));
        }
        else
        {
            contentFilters.Add(b.Eq(f => f.ExternalId, null));
        }

        return await collection.Find(b.And(contentFilters)).FirstOrDefaultAsync();
    }

    #endregion
    
}