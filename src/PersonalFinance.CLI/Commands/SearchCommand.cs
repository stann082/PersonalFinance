using PersonalFinance.CLI.Options;
using PersonalFinance.Domain.Services;

namespace PersonalFinance.CLI.Commands;

public class SearchCommand(SearchOptions options, IStatementService service)
{

    #region Public Methods

    public async Task<int> Execute()
    {
        return await Task.FromResult(0);
    }

    #endregion

}
