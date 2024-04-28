using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;

namespace PersonalFinance.Application.Services;

public class StatementService : IStatementService
{

    public async Task Save(Transaction[] transactions)
    {
        // TODO: Implement this
        await Task.CompletedTask;
    }
    
}
