using CommandLine;
using PersonalFinance.Domain.Models;

namespace PersonalFinance.CLI.Options;

[Verb("search", HelpText = "Search through your transactions.")]
public class SearchOptions : ISearchOptions
{
    #region Properties

    [Option('f', "filter", HelpText = "Filters by amount, balance, date (e.g., -f dateAfter=2024-02-05&dateBefore=2024-04-07&balance=100.")]
    public string Filter { get; set; }
    
    [Option('q', "query", HelpText = "Search for a specific item.")]
    public string Query { get; set; }

    #endregion

}
