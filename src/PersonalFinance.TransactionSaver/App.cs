using PersonalFinance.Domain.Models;
using PersonalFinance.Infrastructure;

public class App(ITransactionCategorizer transactionCategorizer)
{

    #region Public Methods

    public async Task<int> RunApp(IEnumerable<string> args)
    {
        string? oneDrivePath = Environment.GetEnvironmentVariable("OneDrive");
        if (oneDrivePath == null)
        {
            Console.WriteLine("OneDrive environment variable is not set.");
            return 1;
        }

        string financesPath = Path.Combine(oneDrivePath, "Documents", "personal", "finances");
        string transactionsMapFilePath = Path.Combine(financesPath, "transactions-map.csv");
        if (!File.Exists(transactionsMapFilePath))
        {
            Console.WriteLine($"File {transactionsMapFilePath} not found");
            return 1;
        }

        string statementFilePath = Path.Combine(financesPath, "stmt-full.csv");
        if (!File.Exists(statementFilePath))
        {
            Console.WriteLine($"File {statementFilePath} not found");
            return 1;
        }

        ITransactionMapper[] transactionMapper = await BankOfAmericaStatementParser.Parse(statementFilePath);
        Transaction[] transactions = transactionMapper.Select(t => new Transaction(t)).ToArray();
        await transactionCategorizer.CategorizeTransactions(transactionsMapFilePath, transactions);
        return 0;
    }

    #endregion

}
