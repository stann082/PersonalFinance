using PersonalFinance.CLI.Options;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;
using PersonalFinance.Domain.Util;

namespace PersonalFinance.CLI.Commands;

public class EditCommand(EditOptions options, IStatementService service)
{

    #region Public Methods

    public async Task<int> Execute()
    {
        var transactions = await LoadTransactionsAsync();

        if (transactions.Length == 0)
        {
            Console.WriteLine("No transactions found.");
            return 0;
        }

        Console.WriteLine($"Found {transactions.Length} transaction(s). Starting edit session...\n");

        var pendingEdits = new List<(Transaction Transaction, List<string> Changes)>();
        bool saveAndExit = false;

        for (int i = 0; i < transactions.Length && !saveAndExit; i++)
        {
            var t = transactions[i];
            var changes = new List<string>();

            PrintTransaction(i + 1, transactions.Length, t);

            // --- Recurring prompt (debits only) ---
            if (t.IsRecurring.HasValue)
            {
                Console.Write($"  Recurring (currently: {(t.IsRecurring.Value ? "Yes" : "No")}) [y=yes / n=no / enter=keep]: ");
                var recurringInput = Console.ReadLine()?.Trim().ToLower();
                bool? newRecurring = recurringInput switch
                {
                    "y" => true,
                    "n" => false,
                    _ => null
                };
                if (newRecurring.HasValue && newRecurring.Value != t.IsRecurring.Value)
                {
                    changes.Add($"Recurring: {(t.IsRecurring.Value ? "Yes" : "No")} → {(newRecurring.Value ? "Yes" : "No")}");
                    t.IsRecurring = newRecurring.Value;
                    Console.WriteLine($"  → Recurring set to {(t.IsRecurring.Value ? "Yes" : "No")}");
                }
            }

            Console.WriteLine();

            // --- Main menu ---
            while (true)
            {
                Console.Write("  [c]ontinue editing  [n]ext  [r]eview  [s]ave: ");
                var choice = Console.ReadLine()?.Trim().ToLower();

                if (choice is "n" or "")
                    break;

                if (choice == "s")
                {
                    saveAndExit = true;
                    break;
                }

                if (choice == "c")
                {
                    Console.Write($"  Description (enter to keep \"{t.Description}\"): ");
                    var newDesc = Console.ReadLine();
                    if (!string.IsNullOrEmpty(newDesc) && newDesc != t.Description)
                    {
                        changes.Add($"Description: \"{t.Description}\" → \"{newDesc}\"");
                        t.Description = newDesc;
                        Console.WriteLine($"  → Description updated");
                    }

                    Console.Write($"  Category (enter to keep \"{t.Category}\"): ");
                    var newCat = Console.ReadLine();
                    if (!string.IsNullOrEmpty(newCat) && newCat != t.Category)
                    {
                        changes.Add($"Category: \"{t.Category}\" → \"{newCat}\"");
                        t.Category = newCat;
                        Console.WriteLine($"  → Category updated");
                    }

                    Console.WriteLine();
                    continue;
                }

                if (choice == "r")
                {
                    PrintReview(pendingEdits);
                    Console.WriteLine();
                    continue;
                }

                Console.WriteLine("  Unknown option.");
            }

            if (changes.Count > 0)
                pendingEdits.Add((t, changes));

            Console.WriteLine();
        }

        if (!saveAndExit)
        {
            if (pendingEdits.Count == 0)
            {
                Console.WriteLine("No changes made.");
                return 0;
            }

            PrintReview(pendingEdits);
            Console.Write($"\n{pendingEdits.Count} pending edit(s). Save? [y/n]: ");
            saveAndExit = Console.ReadLine()?.Trim().ToLower() == "y";
        }

        if (saveAndExit)
        {
            if (pendingEdits.Count == 0)
            {
                Console.WriteLine("No changes to save.");
                return 0;
            }

            foreach (var (t, _) in pendingEdits)
                await service.UpdateAsync(t);

            Console.WriteLine($"Saved {pendingEdits.Count} change(s).");
        }
        else
        {
            Console.WriteLine("Discarded.");
        }

        return 0;
    }

    #endregion

    #region Helper Methods

    private async Task<Transaction[]> LoadTransactionsAsync()
    {
        Transaction[] allTransactions = await service.GetAsync(options);

        DateTime startDate = DateTime.MinValue;
        DateTime stopDate = DateTime.MaxValue;

        if (!string.IsNullOrEmpty(options.PayPeriod))
        {
            var dateRange = await new PayPeriodTransactionsProvider(service).GetDateRangeAsync(options.PayPeriod);
            startDate = dateRange.StartDate ?? DateTime.MinValue;
            stopDate = dateRange.StopDate ?? DateTime.MaxValue;
        }

        string after = options.GetFilterValue("after");
        if (!string.IsNullOrEmpty(after) && DateTime.TryParse(after, out var afterDate))
            startDate = afterDate;

        string before = options.GetFilterValue("before");
        if (!string.IsNullOrEmpty(before) && DateTime.TryParse(before, out var beforeDate))
            stopDate = beforeDate;

        var periodTransactions = allTransactions
            .Where(t => t.Date >= startDate && t.Date <= stopDate)
            .ToArray();

        IEnumerable<Transaction> filtered = periodTransactions;

        bool? isRecurring = bool.TryParse(options.GetFilterValue("recurring"), out var r) ? r : null;
        if (isRecurring.HasValue)
            filtered = filtered.Where(t => t.IsRecurring == isRecurring.Value);

        if (!string.IsNullOrEmpty(options.Query))
            filtered = filtered.Where(t => t.Description != null &&
                t.Description.Contains(options.Query, StringComparison.OrdinalIgnoreCase));

        return filtered.OrderBy(t => t.Date).ToArray();
    }

    private static void PrintTransaction(int index, int total, Transaction t)
    {
        var amount = $"${Math.Abs(t.Amount):N2}".PadLeft(10);
        var recurring = t.IsRecurring == true ? "[Recurring]" : "";
        Console.ForegroundColor = t.IsDeposit ? ConsoleColor.Red : ConsoleColor.Gray;
        Console.WriteLine($"[{index}/{total}] {t.Date:yyyy-MM-dd}  {amount}  {t.Description}  [{t.Category}]  {recurring}");
        Console.ResetColor();
    }

    private static void PrintReview(List<(Transaction Transaction, List<string> Changes)> pendingEdits)
    {
        if (pendingEdits.Count == 0)
        {
            Console.WriteLine("  No pending edits.");
            return;
        }

        Console.WriteLine("  Pending edits:");
        foreach (var (t, changes) in pendingEdits)
        {
            Console.WriteLine($"    {t.Date:yyyy-MM-dd}  {t.Description}");
            foreach (var change in changes)
                Console.WriteLine($"      • {change}");
        }
    }

    #endregion

}
