using CommandLine;
using PersonalFinance.Domain.Models;

namespace PersonalFinance.CLI.Options;

[Verb("search", HelpText = "Search through your transactions.")]
public class SearchOptions : ISearchOptions
{

    #region Properties - Search Options

    [Option('f', "filter", HelpText = "Filters by amount, balance, date (e.g., -f \"after=2024-02-05&before=2024-04-07&balance=100\"")]
    public string Filter { get; set; } = string.Empty;

    [Option('q', "query", HelpText = "Search for a specific item.")]
    public string Query { get; set; } = string.Empty;

    [Option("pay-period", HelpText = "Enter the pay period (e.g., \"September2024\"")]
    public string PayPeriod { get; set; } = string.Empty;

    #endregion

    #region Properties

    private Dictionary<string, string> FilterMap => CreateFilterMap();

    #endregion

    #region Public Methods

    public string? GetFilterValue(string key)
    {
        if (FilterMap.Count == 0)
        {
            return string.Empty;
        }

        return FilterMap.TryGetValue(key, out string? value) ? value : string.Empty;
    }

    #endregion

    #region Helper Methods

    private Dictionary<string, string> CreateFilterMap()
    {
        if (string.IsNullOrEmpty(Filter))
        {
            return new Dictionary<string, string>();
        }

        return Filter.Split('&')
            .Select(part => part.Split('='))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0], parts => parts[1]);
    }

    #endregion

}
