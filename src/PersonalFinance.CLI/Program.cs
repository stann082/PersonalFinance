using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using PersonalFinance.Application.Services;
using PersonalFinance.Domain.Services;
using PersonalFinance.Infrastructure;
using PersonalFinance.Infrastructure.SimpleFin;
using Serilog;

namespace PersonalFinance.CLI;

public static class Program
{

    #region Main Method

    public static async Task<int> Main(string[] args)
    {
        string baseLogPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string logFilePath = Path.Combine(baseLogPath, "logs", "personal-finance", "usage.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(logFilePath, shared: true, rollingInterval: RollingInterval.Day)
            .CreateLogger();
        
        var services = new ServiceCollection()
            .AddSingleton<App>()
            .AddSingleton<IMongoDatabase>(_ => new MongoClient("mongodb://localhost:27017")
                .GetDatabase("financial_data"))
            .AddScoped<IBankStatementParser, BankStatementParser>()
            .AddScoped<IStatementService, StatementService>()
            .AddScoped<ITransactionCategorizer, TransactionCategorizer>();
        services.AddHttpClient<ISimpleFinClient, SimpleFinClient>();

        var provider = services.BuildServiceProvider();
        return await provider.GetService<App>()?.RunApp(args)!;
    }

    #endregion

}
