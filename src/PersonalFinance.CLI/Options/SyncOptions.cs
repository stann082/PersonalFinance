using CommandLine;
using PersonalFinance.Domain.Models;

namespace PersonalFinance.CLI.Options;

[Verb("sync", HelpText = "Sync transactions from SimpleFIN Bridge.")]
public class SyncOptions : ISyncOptions
{
    [Option('d', "days", Default = 30, HelpText = "How many days back to pull. Capped at 90 by SimpleFIN.")]
    public int Days { get; set; }

    [Option("pending", HelpText = "Include pending transactions. Off by default; their ids change when they post.")]
    public bool IncludePending { get; set; }

    [Option("dry-run", HelpText = "Show what would be saved without writing to the database.")]
    public bool DryRun { get; set; }

    [Option('v', "verbose", HelpText = "Show details")]
    public bool Verbose { get; set; }
}
