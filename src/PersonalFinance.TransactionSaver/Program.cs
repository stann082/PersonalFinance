using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using PersonalFinance.Application.Services;
using PersonalFinance.Domain.Services;
using PersonalFinance.Infrastructure;

namespace PersonalFinance.TransactionSaver;

public static class Program
{

    #region Main Method

    public static async Task<int> Main(string[] args)
    {
        var services = new ServiceCollection()
            .AddSingleton<App>()
            .AddSingleton<IMongoClient>(_ => new MongoClient("mongodb://localhost:27017"))
            .AddScoped<IBankOfAmericaStatementParser, BankOfAmericaStatementParser>()
            .AddScoped<IStatementService, StatementService>()
            .AddScoped<ITransactionCategorizer, TransactionCategorizer>()
            .BuildServiceProvider();
        return await services.GetService<App>()?.RunApp(args)!;
    }

    #endregion

}
