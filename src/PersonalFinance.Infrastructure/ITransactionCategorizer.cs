using PersonalFinance.Domain.Models;

namespace PersonalFinance.Infrastructure;

public interface ITransactionCategorizer
{
    Task CategorizeAsync(string modelTrainingFilePath, IEnumerable<IParsedTransactionItem> transactions);
}
