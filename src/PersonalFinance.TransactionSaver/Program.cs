using Microsoft.Extensions.DependencyInjection;
using PersonalFinance.Infrastructure;

namespace PersonalFinance.TransactionSaver;

public static class Program
{

    #region Main Method

    public static async Task<int> Main(string[] args)
    {
        var services = new ServiceCollection()
            .AddSingleton<App>()
            .AddScoped<ITransactionCategorizer, TransactionCategorizer>()
            .BuildServiceProvider();
        return await services.GetService<App>()?.RunApp(args)!;
    }

    #endregion

}
