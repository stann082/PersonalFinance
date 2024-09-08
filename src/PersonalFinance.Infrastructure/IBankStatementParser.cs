using PersonalFinance.Domain.Models;

namespace PersonalFinance.Infrastructure;

public interface IBankStatementParser
{
    Task<IParsedTransactionItem[]> Parse(string filePath);
}
