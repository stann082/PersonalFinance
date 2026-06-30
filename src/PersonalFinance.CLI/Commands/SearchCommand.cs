using PersonalFinance.CLI.Options;
using PersonalFinance.Domain.Misc;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;
using PersonalFinance.Domain.Util;

namespace PersonalFinance.CLI.Commands;

public class SearchCommand(SearchOptions options, IStatementService service)
{

    #region Constants

    private const string PayPeriodDescription = "TIMECLOCK PLUS L DES";

    #endregion

    #region Public Methods

    public async Task<int> Execute()
    {
        IEnumerable<Transaction> allTransactions = await service.GetAsync(options);

        DateTime startDate = DateTime.MinValue;
        DateTime stopDate = DateTime.MaxValue;
        if (!string.IsNullOrEmpty(options.PayPeriod))
        {
            var dateRange = await new PayPeriodTransactionsProvider(service).GetDateRangeAsync(options.PayPeriod);
            startDate = dateRange.StartDate ?? DateTime.MinValue;
            stopDate = dateRange.StopDate ?? DateTime.MaxValue;
        }

        string after = options.GetFilterValue("after");
        if (!string.IsNullOrEmpty(after))
        {
            startDate = DateTime.TryParse(after, out var beforeDate) ? beforeDate : DateTime.MinValue;
        }

        string before = options.GetFilterValue("before");
        if (!string.IsNullOrEmpty(before))
        {
            stopDate = DateTime.TryParse(before, out var afterDate) ? afterDate : DateTime.MaxValue;
        }

        var periodTransactions = allTransactions.Where(t => t.Date >= startDate && t.Date <= stopDate).ToArray();

        IEnumerable<Transaction> filteredTransactions = periodTransactions;
        bool? isRecurring = bool.TryParse(options.GetFilterValue("recurring"), out var recurring) ? recurring : null;
        if (isRecurring.HasValue)
        {
            filteredTransactions = filteredTransactions.Where(t => t.IsRecurring == isRecurring.Value).ToArray();
        }

        var sortByAmount = options.GetFilterValue("amountSort");
        filteredTransactions = sortByAmount switch
        {
            "desc" => filteredTransactions.OrderByDescending(t => t.Amount).ToArray(),
            "asc" => filteredTransactions.OrderBy(t => t.Amount).ToArray(),
            _ => filteredTransactions
        };

        if (!string.IsNullOrEmpty(options.Query))
        {
            filteredTransactions = filteredTransactions.Where(t =>
                t.Description != null &&
                t.Description.Contains(options.Query, StringComparison.OrdinalIgnoreCase));
        }

        var filteredArray = filteredTransactions.ToArray();
        if (options.Export)
        {
            string filePath = Path.Combine(Path.GetTempPath(), $"fin-{DateTime.Now:yyyyMMddHHmmss}.csv");
            
            List<string> fileContent = [ options.ShowBalance ? "Date,Description,Amount,Category,Balance" : "Date,Description,Amount,Category" ];
            foreach (Transaction t in filteredArray)
            {
                t.Description = t.Description?.Replace(',', ' ');
                fileContent.Add(options.ShowBalance
                    ? $"{t.Date.ToShortDateString()},{t.Description},{Math.Abs(t.Amount)},{t.Category},{t.Balance}"
                    : $"{t.Date.ToShortDateString()},{t.Description},{Math.Abs(t.Amount)},{t.Category}");
            }
            
            await File.WriteAllLinesAsync(filePath, fileContent);
            Console.WriteLine($"The transactions were exported to {filePath}");
        }
        else
        {
            int? wrap = int.TryParse(options.GetFilterValue("wrap"), out var wrapLength) ? wrapLength : null;
            FormatAndPrintTransactions(filteredArray, wrap, options.ShowBalance);
            DisplayTotals(periodTransactions, filteredArray);
        }
        
        return await Task.FromResult(0);
    }

    #endregion

    #region Helper Methods

    private static void DisplayTotals(Transaction[] periodTransactions, Transaction[] filteredTransactions)
    {
        var debits = periodTransactions.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount));
        var credits = periodTransactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
        Console.WriteLine($"\n     Money spent: ${Math.Abs(debits):N2}");
        Console.WriteLine($"  Money received: ${credits:N2}");
        Console.WriteLine($"      Money left: ${credits - debits:N2}\n");

        var recurringDebits = filteredTransactions.Where(t => t is { IsRecurring: true }).Sum(t => t.Amount);
        if (recurringDebits != 0) Console.WriteLine($"       Recurring expenses: ${Math.Abs(recurringDebits):N2}");

        var nonRecurringDebits = filteredTransactions.Where(t => t is { IsRecurring: false }).Sum(t => t.Amount);
        if (nonRecurringDebits != 0) Console.WriteLine($"        One-time expenses: ${Math.Abs(nonRecurringDebits):N2}");

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\nMoney received highlighted in red\n");
        Console.ResetColor();
    }

    private static void FormatAndPrintTransactions(Transaction[] transactions, int? wrapLength, bool showBalance = false)
    {
        var length = wrapLength ?? 500;
        var limitTo = length - 3;
        foreach (var transaction in transactions)
        {
            var formattedAmount = $"${Math.Abs(transaction.Amount)}".PadLeft(10);
            var formattedDescription = transaction.Description?.Length > length
                ? transaction.Description[..limitTo] + "..."
                : transaction.Description;

            Console.ForegroundColor = transaction.TransactionType == TransactionType.Credit
                ? ConsoleColor.Red
                : ConsoleColor.Gray;

            var line = showBalance
                ? $"{transaction.Date}\t{formattedAmount}\t{transaction.Balance,12:0.00}\t{formattedDescription}"
                : $"{transaction.Date}\t{formattedAmount}\t{formattedDescription}";
            Console.WriteLine(line);
            Console.ResetColor();
        }
    }

    #endregion

}