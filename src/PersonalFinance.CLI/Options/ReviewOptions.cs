using CommandLine;

namespace PersonalFinance.CLI.Options;

[Verb("review", HelpText = "Review recurring transactions.")]
public class ReviewOptions
{
    
    [Option("start-date", HelpText = "Specify the start date (e.g., 2025-09-25)")]
    public string StartDate { get; set; } = string.Empty;

}