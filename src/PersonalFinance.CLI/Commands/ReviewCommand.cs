using MongoDB.Driver;
using PersonalFinance.Application.Models;
using PersonalFinance.Application.Services;
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
        var hasValidPaths = pathProvider.Validate(true);
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

        ITransaction[] filteredTransactions = allTransactions.Where(t => t.Date >= startDate).ToArray();
        await transactionCategorizer.CategorizeAsync(pathProvider.CategoryModelTrainingDataFilePath, filteredTransactions);

        var pending = new List<PendingUpdate>();

        var transactionsCollection = (service as StatementService)?.GetTransactionsCollection();
        if (transactionsCollection == null)
        {
            Console.WriteLine("No transactions found");
            return 1;
        }

        foreach (ITransaction transaction in filteredTransactions)
        {
            var filter = Builders<Transaction>.Filter.And(
                Builders<Transaction>.Filter.Eq(f => f.Description, transaction.Description),
                Builders<Transaction>.Filter.Eq(f => f.Amount, transaction.Amount),
                Builders<Transaction>.Filter.Eq(f => f.Date, transaction.Date),
                Builders<Transaction>.Filter.Eq(f => f.Balance, transaction.Balance)
            );

            var existingTransaction = await transactionsCollection.Find(filter).FirstOrDefaultAsync();
            if (existingTransaction == null || existingTransaction.IsRecurring == transaction.IsRecurring)
            {
                continue;
            }

            pending.Add(new PendingUpdate
            {
                Existing = existingTransaction,
                DesiredIsRecurring = transaction.IsRecurring
            });
        }

        if (pending.Count == 0)
        {
            Console.WriteLine("No transactions needing attention were found");
            return 0;
        }

        Console.WriteLine($"There are {pending.Count} transactions that need to be reviewed");
        await ReviewAndApplyUpdatesAsync(transactionsCollection, pending);
        return await Task.FromResult(0);
    }

    #endregion

    #region Helper Methods

    private static async Task ReviewAndApplyUpdatesAsync(IMongoCollection<Transaction> collection, IList<PendingUpdate> pending)
    {
        const int pageSize = 10;
        int page = 0;

        while (true)
        {
            var pageItems = pending.Skip(page * pageSize).Take(pageSize).ToList();
            if (pageItems.Count == 0)
            {
                Console.WriteLine("No more items.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"Showing {page * pageSize + 1}-{page * pageSize + pageItems.Count} of {pending.Count}");
            Console.WriteLine("Idx | Date        | Amount     | IsRecurring (current → desired) | Description");
            Console.WriteLine("----+-------------+------------+---------------------------------+------------");

            for (int i = 0; i < pageItems.Count; i++)
            {
                var idx = i + 1;
                var it = pageItems[i];
                Console.WriteLine(
                    $"{idx,3} | {it.Existing?.Date:yyyy-MM-dd} | {it.Existing?.Amount,10:0.00} | {it.Existing?.IsRecurring,5} → {it.DesiredIsRecurring,5}        | {Trunc(it.Existing?.Description, 60)}");
            }

            Console.WriteLine();
            Console.WriteLine("Enter numbers to update (comma-separated),");
            Console.WriteLine("or press Enter to skip page, 'n' next, 'p' prev, 'q' quit:");
            Console.Write("> ");

            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input))
            {
                page++;
                continue;
            }

            if (input.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (input.Equals("n", StringComparison.OrdinalIgnoreCase))
            {
                page = Math.Min(page + 1, (pending.Count - 1) / pageSize);
                continue;
            }

            if (input.Equals("p", StringComparison.OrdinalIgnoreCase))
            {
                page = Math.Max(page - 1, 0);
                continue;
            }

            // Parse comma-delimited selection
            var indices = ParseSelection(input, pageItems.Count);
            if (indices.Count == 0)
            {
                Console.WriteLine("No valid indices selected.");
                continue;
            }

            // Prepare bulk updates for selected items on this page
            var writes = new List<WriteModel<Transaction>>(indices.Count);
            writes.AddRange(from oneBased in indices
                select pageItems[oneBased - 1]
                into item
                let idFilter = Builders<Transaction>.Filter.Eq(x => x.Id, item.Existing?.Id)
                let update = Builders<Transaction>.Update.Set(x => x.IsRecurring, item.DesiredIsRecurring)
                select new UpdateOneModel<Transaction>(idFilter, update));

            if (writes.Count > 0)
            {
                var result = await collection.BulkWriteAsync(writes);
                Console.WriteLine($"Updated {result.ModifiedCount} item(s).");
            }
        }

        static List<int> ParseSelection(string input, int max)
        {
            var list = new List<int>();
            foreach (var token in input.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(token.Trim(), out var n) && n >= 1 && n <= max)
                {
                    list.Add(n);
                }
            }

            return list.Distinct().OrderBy(i => i).ToList();
        }

        static string Trunc(string? s, int max)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            return s.Length <= max ? s : s.Substring(0, max - 1) + "…";
        }
    }

    #endregion

    #region Helper Classes

    private sealed class PendingUpdate
    {
        public Transaction? Existing { get; init; }
        public bool DesiredIsRecurring { get; init; }
    }

    #endregion
}