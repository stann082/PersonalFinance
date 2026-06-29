using System.Text.RegularExpressions;
using PersonalFinance.CLI.Options;
using PersonalFinance.CLI.Util;
using PersonalFinance.Domain.Misc;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;

namespace PersonalFinance.CLI.Commands;

public class ExportCommand(ExportOptions options, IStatementService service)
{

    #region Constants

    private const string PayPeriodDescription = "TIMECLOCK PLUS L DES:DIRECT DEP";

    #endregion

    #region Public Methods

    public async Task<int> Execute()
    {
        IEnumerable<Transaction> allTransactions = await service.GetAsync();

        DateTime startDate = options.StartDate != string.Empty && DateTime.TryParse(options.StartDate, out var parsedStartDate) ? parsedStartDate : DateTime.MinValue;
        DateTime stopDate = options.StopDate != string.Empty && DateTime.TryParse(options.StopDate, out var parsedStopDate) ? parsedStopDate : DateTime.MaxValue;

        var periodTransactions = allTransactions.Where(t => t.Date >= startDate && t.Date <= stopDate).ToArray();

        IEnumerable<Transaction> filteredTransactions = periodTransactions;
        if (!string.IsNullOrEmpty(options.Query))
        {
            filteredTransactions = filteredTransactions.Where(t =>
                t.Description != null &&
                t.Description.Contains(options.Query, StringComparison.OrdinalIgnoreCase));
        }

        var filteredArray = filteredTransactions.ToArray();
        string filePath = Path.Combine(Path.GetTempPath(), $"fin-{DateTime.Now:yyyyMMddHHmmss}.csv");

        List<string> fileContent = [options.ShowBalance ? "Date,Description,Amount,Category,Balance" : "Date,Description,Amount,Category"];
        foreach (Transaction t in filteredArray)
        {
            t.Description = t.Description?.Replace(',', ' ');

            if (options.Obscure)
            {
                t.Description = PersonalNameObscurer.Obscure(t.Description, "Stan", "Bennett");
            }

            fileContent.Add(options.ShowBalance
                ? $"{t.Date:d},{t.Description},{Math.Abs(t.Amount)},{t.Category},{t.Balance}"
                : $"{t.Date:d},{t.Description},{Math.Abs(t.Amount)},{t.Category}");
        }

        await File.WriteAllLinesAsync(filePath, fileContent);
        Console.WriteLine($"The transactions were exported to {filePath}");
        return await Task.FromResult(0);
    }

    #endregion

}