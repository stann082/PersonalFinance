using CommandLine;
using PersonalFinance.Domain.Models;

namespace PersonalFinance.CLI.Options;

[Verb("edit", HelpText = "Interactively edit transactions.")]
public class EditOptions : ISearchOptions
{

    #region Properties

    [Option('f', "filter", HelpText = "Filter by date, recurring, etc. (e.g., -f \"after=2024-02-05&before=2024-04-07\")")]
    public string Filter { get; set; } = string.Empty;

    [Option('q', "query", HelpText = "Search for transactions containing this text.")]
    public string Query { get; set; } = string.Empty;

    [Option("pay-period", HelpText = "Limit to a pay period (e.g., \"June2025\").")]
    public string PayPeriod { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; } = DateTime.MinValue;
    public DateTime? StopDate { get; set; } = DateTime.MaxValue;

    #endregion

    #region Public Methods

    public string GetFilterValue(string key)
    {
        var map = CreateFilterMap();
        return map.TryGetValue(key, out string? value) ? value : string.Empty;
    }

    #endregion

    #region Helper Methods

    private Dictionary<string, string> CreateFilterMap()
    {
        if (string.IsNullOrEmpty(Filter))
            return new Dictionary<string, string>();

        return Filter.Split('&')
            .Select(part => part.Split('='))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0], parts => parts[1]);
    }

    #endregion

}
