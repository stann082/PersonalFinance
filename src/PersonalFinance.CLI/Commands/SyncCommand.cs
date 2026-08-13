using PersonalFinance.CLI.Options;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;
using PersonalFinance.Domain.Util;
using PersonalFinance.Infrastructure;
using PersonalFinance.Infrastructure.SimpleFin;

namespace PersonalFinance.CLI.Commands;

public class SyncCommand(SyncOptions options, ISimpleFinClient simpleFinClient, IStatementService statementService,
    ITransactionCategorizer transactionCategorizer)
{

    #region Constants

    /// <summary>SimpleFIN rejects ranges wider than this.</summary>
    private const int MaxDays = 90;

    #endregion

    #region Public Methods

    public async Task<int> Execute()
    {
        var credentials = new SimpleFinCredentials();
        var hasCredentials = credentials.Validate();
        if (!hasCredentials.IsValid)
        {
            Console.WriteLine(hasCredentials.Error);
            return 1;
        }

        var pathProvider = new PathProvider();
        var hasValidPaths = pathProvider.Validate(true);
        if (!hasValidPaths.IsValid)
        {
            Console.WriteLine(hasValidPaths.Error);
            return 1;
        }

        int days = Math.Clamp(options.Days, 1, MaxDays);
        if (days != options.Days)
        {
            Console.WriteLine($"Requested {options.Days} days; using {days} (SimpleFIN allows at most {MaxDays}).");
        }

        var endDate = DateTimeOffset.Now;
        var startDate = endDate.AddDays(-days);
        Console.WriteLine($"Fetching transactions from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}...");

        SimpleFinAccountSet accountSet;
        try
        {
            accountSet = await simpleFinClient.GetAccountsAsync(startDate, endDate, options.IncludePending);
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or TaskCanceledException)
        {
            Console.WriteLine($"Failed to reach SimpleFIN: {ex.Message}");
            return 1;
        }

        PrintErrors(accountSet);

        SimpleFinTransactionItem[] items = FlattenTransactions(accountSet);
        if (items.Length == 0)
        {
            Console.WriteLine("No transactions returned for that window.");
            return 0;
        }

        Console.WriteLine($"Retrieved {items.Length} transactions across {accountSet.Accounts.Length} account(s).");

        await transactionCategorizer.CategorizeAsync(pathProvider.CategoryModelTrainingDataFilePath, items);

        if (options.DryRun)
        {
            PrintDryRun(items);
            return 0;
        }

        await statementService.EnsureIndexesAsync();
        await statementService.SaveAsync(items.Cast<IParsedTransactionItem>().ToArray(), options);
        return 0;
    }

    #endregion

    #region Helper Methods

    private SimpleFinTransactionItem[] FlattenTransactions(SimpleFinAccountSet accountSet)
    {
        return accountSet.Accounts
            .SelectMany(account => account.Transactions
                .Where(t => options.IncludePending || !t.Pending)
                .Select(t => new SimpleFinTransactionItem(account, t)))
            .ToArray();
    }

    /// <summary>
    /// The bridge reports stale institution logins and rate-limit warnings here rather than
    /// as HTTP failures, so these have to be surfaced or they go unnoticed.
    /// </summary>
    private static void PrintErrors(SimpleFinAccountSet accountSet)
    {
        foreach (string error in accountSet.Errors)
        {
            Console.WriteLine($"SimpleFIN: {error}");
        }

        if (accountSet.ErrList == null)
        {
            return;
        }

        foreach (var error in accountSet.ErrList)
        {
            Console.WriteLine($"SimpleFIN [{error.Code}]: {error.Msg}");
        }
    }

    private static void PrintDryRun(SimpleFinTransactionItem[] items)
    {
        Console.WriteLine();
        Console.WriteLine("Date        | Amount      | Category             | Description");
        Console.WriteLine("------------+-------------+----------------------+------------");

        foreach (var item in items.OrderBy(i => DateTime.TryParse(i.Date, out var d) ? d : DateTime.MinValue))
        {
            decimal amount = decimal.TryParse(item.Amount, out var parsed) ? parsed : 0;
            Console.WriteLine($"{item.Date,-11} | {amount,11:0.00} | {Truncate(item.Category, 20),-20} | {item.Description}");
        }

        Console.WriteLine();
        Console.WriteLine($"Dry run: {items.Length} transactions would be saved. Nothing was written.");
    }

    private static string Truncate(string? value, int max)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Length <= max ? value : string.Concat(value.AsSpan(0, max - 1), "…");
    }

    #endregion

}
