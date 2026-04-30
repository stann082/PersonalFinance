using System.Text.RegularExpressions;
using PersonalFinance.CLI.Options;
using PersonalFinance.Domain.Misc;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;
using PersonalFinance.Domain.Util;

namespace PersonalFinance.CLI.Commands;

public class ExportCommand(ExportOptions options, IStatementService service)
{

    #region Constants

    private const string PayPeriodDescription = "TIMECLOCK PLUS L DES:DIRECT DEP";

    #endregion

    #region Public Methods

    public async Task<int> Execute()
    {
        IEnumerable<ITransaction> allTransactions = await service.GetAsync();

        DateTime startDate = options.StartDate != string.Empty && DateTime.TryParse(options.StartDate, out var parsedStartDate) ? parsedStartDate : DateTime.MinValue;
        DateTime stopDate = options.StopDate != string.Empty && DateTime.TryParse(options.StopDate, out var parsedStopDate) ? parsedStopDate : DateTime.MaxValue;

        var periodTransactions = allTransactions.Where(t => t.Date >= startDate && t.Date <= stopDate).ToArray();

        IEnumerable<ITransaction> filteredTransactions = periodTransactions;
        if (!string.IsNullOrEmpty(options.Query))
        {
            filteredTransactions = filteredTransactions.Where(t =>
                t.Description != null &&
                t.Description.Contains(options.Query, StringComparison.OrdinalIgnoreCase));
        }

        var filteredArray = filteredTransactions.ToArray();
        string filePath = Path.Combine(Path.GetTempPath(), $"fin-{DateTime.Now:yyyyMMddHHmmss}.csv");

        List<string> fileContent = [options.ShowBalance ? "Date,Description,Amount,Category,Balance" : "Date,Description,Amount,Category"];
        foreach (ITransaction t in filteredArray)
        {
            t.Description = t.Description?.Replace(',', ' ');

            if (options.Obscure)
            {
                t.Description = ObscurePersonalName(t.Description, "Stan", "Bennett");
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
    #region Helper Methods

    private static string ObscurePersonalName(string? description, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return description ?? string.Empty;
        }

        var firstToken = Regex.Escape(firstName);
        var lastToken = Regex.Escape(lastName);

        var firstVariant = $@"{firstToken}[A-Za-z]*";
        var middleSuffix = @"(?:[\s\p{P}]+[A-Za-z]\.?)?";
        var separator = @"[\s\p{P}]*";

        var patterns = new[]
        {
            $@"\b{firstVariant}{middleSuffix}{separator}{lastToken}\b",
            $@"\b{lastToken}{separator}{firstVariant}{middleSuffix}\b",
            $@"\b{lastToken}\s*,\s*{firstVariant}{middleSuffix}\b"
        };

        var regex = new Regex(string.Join("|", patterns), RegexOptions.IgnoreCase);
        return regex.Replace(description, "[REDACTED]");
    }

    #endregion

}