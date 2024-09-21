using CommandLine;
using PersonalFinance.Domain.Models;

namespace PersonalFinance.CLI.Options;

[Verb("save", HelpText = "Save transactions in the database.")]
public class SaveOptions : ISaveOptions
{
    [Option('v', "verbose", HelpText = "Show details")]
    public bool Verbose { get; set; }
}
