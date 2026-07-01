using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using PersonalFinance.Application.Services;
using PersonalFinance.Domain.Services;
using PersonalFinance.UI.ViewModels;
using PersonalFinance.UI.Views;

namespace PersonalFinance.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddScoped<IStatementService, StatementService>();
        var mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING") ?? "mongodb://localhost:27017";
        builder.Services.AddSingleton<IMongoDatabase>(_ => new MongoClient(mongoConnectionString).GetDatabase("financial_data"));
        builder.Services.AddTransient<TransactionsViewModel>();
        builder.Services.AddTransient<TransactionsPage>();
        builder.Services.AddTransient<TransactionDetailsViewModel>();
        builder.Services.AddTransient<TransactionDetailsPage>();
        return builder.Build();
    }
}
