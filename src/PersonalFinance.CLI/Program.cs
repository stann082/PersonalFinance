using Microsoft.Extensions.DependencyInjection;
using PersonalFinance.Application.Services;
using PersonalFinance.Domain.Services;

namespace PersonalFinance.CLI;

public static class Program
{

    #region Main Method

    public static async Task<int> Main(string[] args)
    {
        var services = new ServiceCollection()
            .AddSingleton<App>()
            .AddScoped<ITransactionAnalysisService, TransactionAnalysisService>()
            .BuildServiceProvider();
        return await services.GetService<App>()?.RunApp(args)!;
    }

    #endregion

}
