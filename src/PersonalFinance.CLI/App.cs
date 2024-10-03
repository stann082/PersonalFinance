using CommandLine;
using PersonalFinance.CLI.Commands;
using PersonalFinance.CLI.Options;
using PersonalFinance.Domain.Services;
using PersonalFinance.Infrastructure;

namespace PersonalFinance.CLI;

public class App(
    IBankStatementParser statementParser,
    IStatementService statementService,
    ITransactionCategorizer transactionCategorizer)
{

    #region Public Methods

    public async Task<int> RunApp(IEnumerable<string> args)
    {
        return await Parser.Default.ParseArguments<SaveOptions, SearchOptions>(args)
            .MapResult(
                async (SaveOptions opts) => await new SaveCommand(opts,
                    statementParser,
                    statementService,
                    transactionCategorizer).Execute(),
                async (SearchOptions opts) => await new SearchCommand(opts, statementService).Execute(),
                _ => Task.FromResult(1));
    }

    #endregion

}
