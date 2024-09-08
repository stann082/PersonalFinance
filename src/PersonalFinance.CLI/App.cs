using CommandLine;
using PersonalFinance.CLI.Commands;
using PersonalFinance.CLI.Options;
using PersonalFinance.Domain.Services;

namespace PersonalFinance.CLI;

public class App(ITransactionAnalysisService analysisService, IStatementService statementService)
{

    #region Public Methods

    public async Task<int> RunApp(IEnumerable<string> args)
    {
        return await Parser.Default.ParseArguments<SearchOptions>(args)
            .MapResult(
                async opts => await new SearchCommand(opts, statementService).Execute(),
                _ => Task.FromResult(1));
    }

    #endregion

}
