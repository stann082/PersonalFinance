using PersonalFinance.CLI.Options;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;

namespace PersonalFinance.CLI.Commands;

public class SearchCommand(SearchOptions options, IStatementService service)
{

    #region Public Methods

    public async Task<int> Execute()
    {
        IEnumerable<ITransaction> transactions = await service.GetAsync();

        DateTime startDate = DateTime.TryParse(options.GetFilterValue("after"), out DateTime beforeDate) ? beforeDate : DateTime.MinValue;
        DateTime stopDate = DateTime.TryParse(options.GetFilterValue("before"), out DateTime afterDate) ? afterDate : DateTime.MaxValue;
        bool? isRecurring = bool.TryParse(options.GetFilterValue("recurring"), out bool recurring) ? recurring : null;
        transactions = transactions.Where(t => t.Date >= startDate && t.Date <= stopDate);

        if (isRecurring.HasValue)
        {
            transactions = transactions.Where(t => t.IsRecurring == isRecurring.Value);
        }
        
        int? wrap = int.TryParse(options.GetFilterValue("wrap"), out int wrapLength) ? wrapLength : null;
        FormatAndPrintTransactions(transactions, wrap);
        
        return await Task.FromResult(0);
    }

    #endregion

    #region Helper Methods

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
        foreach (var output in from t in parsedTransactions
                 let formattedAmount = t.Amount.PadLeft(maxAmountLength)
                 let formattedDescription = t.Description.Length > length
                     ? t.Description[..limitTo] + "..."
                     : t.Description select $"{t.Date}\t{formattedAmount}\t{formattedDescription}")
        {
            Console.WriteLine(output);
        }
    }

    #endregion

}
