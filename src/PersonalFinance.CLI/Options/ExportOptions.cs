using CommandLine;

namespace PersonalFinance.CLI.Options;

[Verb("export", HelpText = "Export transactions.")]
public class ExportOptions
{
    
    [Option("start-date", HelpText = "Specify the start date (e.g., 2025-09-25)")]
    public string StartDate { get; set; } = string.Empty;
    
    [Option("stop-date", HelpText = "Specify the stop date (e.g., 2025-09-25)")]
    public string StopDate { get; set; } = string.Empty;
    
    [Option('o', "obscure", HelpText = "Removes sensitive information from the output (e.g., replaces PII in descriptions with X. Default: true).")]
    public bool Obscure { get; set; } = true;

    [Option('q', "query", HelpText = "Search for a specific item.")]
    public string Query { get; set; } = string.Empty;

    [Option("show-balance", HelpText = "Show balance column")]
    public bool ShowBalance { get; set; } = false;


}