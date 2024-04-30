using PersonalFinance.Domain.Models;

namespace PersonalFinance.Domain.Services;

public interface IStatementService
{
    Task SaveAsync(Transaction[] transactions);
}
