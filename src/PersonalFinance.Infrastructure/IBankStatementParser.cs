using PersonalFinance.Domain.Models;

namespace PersonalFinance.Infrastructure;

public interface IBankStatementParser
{
    Task<ITransactionMap[]> Parse(string filePath);
}
