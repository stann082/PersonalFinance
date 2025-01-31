using CommandLine;
using PersonalFinance.Domain.Models;

namespace PersonalFinance.CLI.Options;

[Verb("remove", HelpText = "Remove selected transactions from the database.")]
public class RemoveOptions : IRemoveOptions
{
    [Option('d', "duplicates", HelpText = "Remove duplicate items")]
    public bool DuplicateItems { get; set; }
}
