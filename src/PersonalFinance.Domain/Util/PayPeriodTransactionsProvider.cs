using PersonalFinance.Domain.Misc;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;

namespace PersonalFinance.Domain.Util;

public class PayPeriodTransactionsProvider(IStatementService service)
{
    #region Public Methods

    public async Task<Transaction[]> GetTransactionsAsync(string payPeriod)
    {
        var dateRange = await GetDateRangeAsync(payPeriod);
        if (dateRange.StartDate is null && dateRange.StopDate is null)
        {
            return [];
        }
        
        return await service.GetAsync(dateRange.StartDate, dateRange.StopDate);
    }

    public async Task<(DateTime? StartDate, DateTime? StopDate)> GetDateRangeAsync(string payPeriod)
    {
        var (windowStart, windowEnd) = PayPeriodWindow.GetWindow(payPeriod);
        Transaction[] windowTransactions = await service.GetAsync(windowStart, windowEnd);
        (string? startDateId, string? endDateId) = PayPeriodCalculator.Calculate(windowTransactions, payPeriod, Constants.PayPeriodDescription);
        if (startDateId is null && endDateId is null)
        {
            return (null, null);
        }

        var startDate = windowTransactions.FirstOrDefault(t => t.TransactionId == startDateId)?.Date ?? DateTime.MinValue;
        var stopDate = windowTransactions.FirstOrDefault(t => t.TransactionId == endDateId)?.Date ?? DateTime.MaxValue;
        return (startDate, stopDate);
    }

    #endregion
}
