using PersonalFinance.Domain.Services;

namespace PersonalFinance.CLI;

public class App(ITransactionAnalysisService transactionAnalysisService)
{

    #region Public Methods

    public async Task<int> RunApp(IEnumerable<string> args)
    {
        await transactionAnalysisService.Analyze();
        return await Task.FromResult(0);
    }

    #endregion

}
