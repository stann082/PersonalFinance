using PersonalFinance.CLI.Options;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;

namespace PersonalFinance.CLI.Commands;

public class SearchCommand(SearchOptions options, IStatementService service)
{

    #region Public Methods

    public async Task<int> Execute()
    {
        IEnumerable<ITransaction> allTransactions = await service.GetAsync();

        DateTime startDate = DateTime.TryParse(options.GetFilterValue("after"), out DateTime beforeDate) ? beforeDate : DateTime.MinValue;
        DateTime stopDate = DateTime.TryParse(options.GetFilterValue("before"), out DateTime afterDate) ? afterDate : DateTime.MaxValue;
        ITransaction[] periodTransactions = allTransactions.Where(t => t.Date >= startDate && t.Date <= stopDate).ToArray();

        IEnumerable<ITransaction> filteredTransactions = periodTransactions;
        bool? isRecurring = bool.TryParse(options.GetFilterValue("recurring"), out bool recurring) ? recurring : null;
        if (isRecurring.HasValue)
        {
            filteredTransactions = filteredTransactions.Where(t => t.IsRecurring == isRecurring.Value).ToArray();
        }

        int? wrap = int.TryParse(options.GetFilterValue("wrap"), out int wrapLength) ? wrapLength : null;
        FormatAndPrintTransactions(filteredTransactions, wrap);

        DisplayTotals(periodTransactions, filteredTransactions.ToArray());
        return await Task.FromResult(0);
    }

    #endregion

    #region Helper Methods

    private static void DisplayTotals(ITransaction[] periodTransactions, ITransaction[] filteredTransactions)
    {
        var debits = periodTransactions.Where(t => t.Amount < 0).Select(t => Math.Abs(t.Amount)).Sum();
        var credits = periodTransactions.Where(t => t.Amount > 0).Select(t => t.Amount).Sum();
        Console.WriteLine($"\n   Total debits: ${Math.Abs(debits):N2}");
        Console.WriteLine($"  Total credits: ${credits:N2}");
        Console.WriteLine($"\n            Net balance: ${credits - debits:N2}");

        var recurringDebits = filteredTransactions.Where(t => t is { IsRecurring: true })
            .Select(t => t.Amount).Sum();
        if (recurringDebits != 0)
        {
            Console.WriteLine($"       Recurring debits: ${Math.Abs(recurringDebits):N2}");
        }

        var nonRecurringDebits = filteredTransactions.Where(t => t is { IsRecurring: false })
            .Select(t => t.Amount).Sum();
        if (nonRecurringDebits != 0)
        {
            Console.WriteLine($"   Non-recurring debits: ${Math.Abs(nonRecurringDebits):N2}");
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\nCredits are in highlighted in red\n");
        Console.ResetColor();
    }

    private static void FormatAndPrintTransactions(IEnumerable<ITransaction> transactions, int? wrapLength)
    {
        var parsedTransactions = transactions.Select(t => t.ToString()).Select(t =>
        {
            var parts = t?.Split([':'], 2);
            var date = parts?[0].Trim();
            var details = parts?[1].Trim();
            var amountEndIndex = details!.IndexOf(' ');
            var amount = details[..amountEndIndex].Trim();
            var description = details[amountEndIndex..].Trim();
            return new { Date = date, Amount = amount, Description = description };
        }).ToList();

        int length = wrapLength ?? 500;
        int limitTo = length - 3;
        int maxAmountLength = parsedTransactions.Max(t => t.Amount.Length);

        foreach (var t in parsedTransactions)
        {
            string formattedAmount = t.Amount.PadLeft(maxAmountLength);
            string formattedDescription = t.Description.Length > length
                ? t.Description[..limitTo] + "..."
                : t.Description;

            bool isCredit = !formattedAmount.Contains('-');
            Console.ForegroundColor = isCredit ? ConsoleColor.Red : ConsoleColor.Gray;
            Console.WriteLine($"{t.Date}\t{formattedAmount}\t{formattedDescription}");
            Console.ResetColor();
        }
    }

    #endregion

}
