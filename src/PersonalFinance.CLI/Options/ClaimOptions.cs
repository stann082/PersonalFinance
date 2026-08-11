using CommandLine;

namespace PersonalFinance.CLI.Options;

[Verb("simplefin-claim", HelpText = "Exchange a one-time SimpleFIN setup token for an access URL.")]
public class ClaimOptions
{
    [Value(0, MetaName = "setup-token", Required = true, HelpText = "The setup token issued by SimpleFIN Bridge.")]
    public string SetupToken { get; set; } = string.Empty;
}
