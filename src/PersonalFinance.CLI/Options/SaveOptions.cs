using CommandLine;
using PersonalFinance.Domain.Models;

namespace PersonalFinance.CLI.Options;

[Verb("save", HelpText = "Save transactions in the database.")]
public class SaveOptions : ISaveOptions
{
    [Option('r', "review", HelpText = "Review transactions before updating")]
    public bool Review { get; set; }
    
    [Option('v', "verbose", HelpText = "Show details")]
    public bool Verbose { get; set; }
}
