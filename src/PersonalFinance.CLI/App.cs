using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;

namespace PersonalFinance.CLI;

public class App(ITransactionAnalysisService analysisService, IStatementService statementService)
{

    #region Public Methods

    public async Task<int> RunApp(IEnumerable<string> args)
    {
        // await analysisService.Analyze();
        ITransaction[] transactions = await statementService.Get();
        return await Task.FromResult(0);
    }

    #endregion

}
