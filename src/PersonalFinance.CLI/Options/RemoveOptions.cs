using CommandLine;
using PersonalFinance.Domain.Models;

namespace PersonalFinance.CLI.Options;

[Verb("remove", HelpText = "Remove selected transactions from the database.")]
public class RemoveOptions : IRemoveOptions
{
    [Option('d', "duplicates", HelpText = "Remove duplicate items")]
    public bool DuplicateItems { get; set; }
    
    [Option("id", HelpText = "Specify transaction id")]
    public string? Id { get; set; }
    
    [Option("show-only", HelpText = "Only show items that could be deleted")]
    public bool ShowOnly { get; set; }
}
