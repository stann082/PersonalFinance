using PersonalFinance.Domain.Models;

namespace PersonalFinance.Infrastructure;

public interface ITransactionCategorizer
{
    Task CategorizeTransactions(string modelTrainingFilePath, IEnumerable<Transaction> transactions);
}
