using PersonalFinance.CLI.Options;
using PersonalFinance.Domain.Misc;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;
using PersonalFinance.Domain.Util;

namespace PersonalFinance.CLI.Commands;

public class SearchCommand(SearchOptions options, IStatementService service)
{

    #region Constants

    private const string PayPeriodDescription = "TIMECLOCK PLUS L DES:DIRECT DEP";

    #endregion
    
    #region Public Methods

    public async Task<int> Execute()
    {
        IEnumerable<ITransaction> allTransactions = await service.GetAsync();

        DateTime startDate = DateTime.MinValue;
        DateTime stopDate = DateTime.MaxValue;
        if (!string.IsNullOrEmpty(options.PayPeriod))
        {
            ITransaction[] allTransactionArray = allTransactions.ToArray();
            var payPeriodCalculator = new PayPeriodCalculator();
            (string? startDateId, string? endDateId) = payPeriodCalculator.Calculate(allTransactionArray, options.PayPeriod, PayPeriodDescription);
            startDate = allTransactionArray.FirstOrDefault(t => t.TransactionId == startDateId)?.Date ?? DateTime.MinValue;
            stopDate = allTransactionArray.FirstOrDefault(t => t.TransactionId == endDateId)?.Date ?? DateTime.MaxValue;
        }

        string? after = options.GetFilterValue("after");
        if (!string.IsNullOrEmpty(after))
        {
            startDate = DateTime.TryParse(after, out var beforeDate) ? beforeDate : DateTime.MinValue;
        }

        string? before = options.GetFilterValue("before");
        if (!string.IsNullOrEmpty(before))
        {
            stopDate = DateTime.TryParse(before, out var afterDate) ? afterDate : DateTime.MaxValue;
        }
        
        var periodTransactions = allTransactions.Where(t => t.Date >= startDate && t.Date <= stopDate).ToArray();

        IEnumerable<ITransaction> filteredTransactions = periodTransactions;
        bool? isRecurring = bool.TryParse(options.GetFilterValue("recurring"), out var recurring) ? recurring : null;
        if (isRecurring.HasValue)
            filteredTransactions = filteredTransactions.Where(t => t.IsRecurring == isRecurring.Value).ToArray();

        var sortByAmount = options.GetFilterValue("amountSort");
        if (sortByAmount == "desc")
            filteredTransactions = filteredTransactions.OrderByDescending(t => t.Amount).ToArray();

        if (!string.IsNullOrEmpty(options.Query))
            filteredTransactions = filteredTransactions.Where(t =>
                t.Description != null && t.Description.Contains(options.Query, StringComparison.OrdinalIgnoreCase));

        var filteredArray = filteredTransactions.ToArray();
        int? wrap = int.TryParse(options.GetFilterValue("wrap"), out var wrapLength) ? wrapLength : null;
        FormatAndPrintTransactions(filteredArray, wrap);
        DisplayTotals(periodTransactions, filteredArray);

        return await Task.FromResult(0);
    }

    #endregion

    #region Helper Methods

    private static void DisplayTotals(ITransaction[] periodTransactions, ITransaction[] filteredTransactions)
    {
        var debits = periodTransactions.Where(t => t.Amount < 0).Select(t => Math.Abs(t.Amount)).Sum();
        var credits = periodTransactions.Where(t => t.Amount > 0).Select(t => t.Amount).Sum();
        Console.WriteLine($"\n     Money spent: ${Math.Abs(debits):N2}");
        Console.WriteLine($"  Money received: ${credits:N2}");
        Console.WriteLine($"      Money left: ${credits - debits:N2}\n");

        var recurringDebits = filteredTransactions.Where(t => t is { IsRecurring: true })
            .Select(t => t.Amount).Sum();
        if (recurringDebits != 0) Console.WriteLine($"       Recurring expenses: ${Math.Abs(recurringDebits):N2}");

        var nonRecurringDebits = filteredTransactions.Where(t => t is { IsRecurring: false })
            .Select(t => t.Amount).Sum();
        if (nonRecurringDebits != 0) Console.WriteLine($"        One-time expenses: ${Math.Abs(nonRecurringDebits):N2}");

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\nMoney received highlighted in red\n");
        Console.ResetColor();
    }

    private static void FormatAndPrintTransactions(ITransaction[] transactions, int? wrapLength)
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
            Console.WriteLine($"{transaction.Date}\t{formattedAmount}\t{formattedDescription}");
            Console.ResetColor();
        }
    }

    #endregion
    
}