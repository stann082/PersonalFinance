using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalFinance.Application.Models;
using PersonalFinance.Application.Services;
using PersonalFinance.Domain.Services;
using PersonalFinance.Infrastructure;

namespace PersonalFinance.TransactionSaver;

public static class Program
{

    #region Main Method

    public static async Task<int> Main(string[] args)
    {
        var connectionString = "Server=localhost;Port=5432;User Id=postgres;Password=gr3B%zPeFtkeD8bpdau^N4j5;Database=finances;";
        var services = new ServiceCollection()
            .AddSingleton<App>()
            .AddScoped<IBankOfAmericaStatementParser, BankOfAmericaStatementParser>()
            .AddScoped<IStatementService, StatementService>()
            .AddScoped<ITransactionCategorizer, TransactionCategorizer>()
            .AddDbContext<TransactionContext>(
                optionsBuilder => optionsBuilder.UseNpgsql(connectionString)
            )
            .BuildServiceProvider();
        return await services.GetService<App>()?.RunApp(args)!;
    }

    #endregion

}
