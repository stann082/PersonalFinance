using PersonalFinance.CLI.Options;
using PersonalFinance.Domain.Services;
using PersonalFinance.Domain.Util;

namespace PersonalFinance.CLI.Commands;

public class ClaimCommand(ClaimOptions options, ISimpleFinClient simpleFinClient)
{

    #region Public Methods

    public async Task<int> Execute()
    {
        string accessUrl;
        try
        {
            accessUrl = await simpleFinClient.ClaimAsync(options.SetupToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or FormatException or TaskCanceledException)
        {
            Console.WriteLine($"Could not claim the setup token: {ex.Message}");
            return 1;
        }

        var credentials = new SimpleFinCredentials();
        var parsed = credentials.Parse(accessUrl);
        if (!parsed.IsValid)
        {
            Console.WriteLine($"SimpleFIN returned something unexpected: {accessUrl}");
            return 1;
        }

        Console.WriteLine("Claimed successfully. A setup token can only be claimed once, so save this now:");
        Console.WriteLine();
        Console.WriteLine(accessUrl);
        Console.WriteLine();
        Console.WriteLine("Persist it for future sessions with:");
        Console.WriteLine($"  setx {SimpleFinCredentials.EnvironmentVariableName} \"{accessUrl}\"");
        Console.WriteLine();
        Console.WriteLine("Then open a new terminal and run 'fin sync --dry-run -v'.");
        return 0;
    }

    #endregion

}
