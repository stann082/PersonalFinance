using PersonalFinance.Domain.Models;

namespace PersonalFinance.Infrastructure;

public interface IBankOfAmericaStatementParser
{
    Task<ITransactionMap[]> Parse(string filePath);
}
