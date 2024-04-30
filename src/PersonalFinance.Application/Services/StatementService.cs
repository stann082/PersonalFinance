using PersonalFinance.Application.Models;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;

namespace PersonalFinance.Application.Services;

public class StatementService(TransactionContext dbContext) : IStatementService
{

    #region Public Methods

    public async Task SaveAsync(Transaction[] transactions)
    {
        TransactionTable[] transactionTables = transactions.Select(CreateTransactionTableModel).ToArray();
        await dbContext.Transactions.AddRangeAsync(transactionTables);
        await dbContext.SaveChangesAsync();
    }

    #endregion

    #region Helper Methods

    private TransactionTable CreateTransactionTableModel(Transaction transaction)
    {
        TransactionTable table = new TransactionTable();
        table.CopyFrom(transaction);
        return table;
    }

    #endregion
    
}
