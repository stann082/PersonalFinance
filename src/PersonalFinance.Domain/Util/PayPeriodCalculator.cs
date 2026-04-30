using System.Globalization;
using PersonalFinance.Domain.Models;

namespace PersonalFinance.Domain.Util;

public static class PayPeriodCalculator
{

    #region Public Methods

    public static (string? startDateId, string? endDateId) Calculate(Transaction[] transactions, string payPeriod, string description)
    {
        if (!DateTime.TryParseExact(payPeriod, "MMMMyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var periodDate))
        {
            throw new ArgumentException("Invalid date format. Use 'MonthYear', e.g., 'September2024'.");
        }

        var firstDayOfPeriod = new DateTime(periodDate.Year, periodDate.Month, 1);
        var firstDayOfNextMonth = firstDayOfPeriod.AddMonths(1);

        var startDateTransaction = transactions
            .Where(t => t.Date < firstDayOfPeriod && t.Description != null && t.Description.Contains(description))
            .OrderByDescending(t => t.Date)
            .FirstOrDefault();

        string? startDateTransactionId = startDateTransaction?.TransactionId;

        var firstNextPeriodPaycheck = transactions
            .Where(t => t.Date >= firstDayOfNextMonth &&
                        t.Description != null &&
                        t.Description.Contains(description))
            .OrderBy(t => t.Date)
            .FirstOrDefault();

        DateTime endOfCurrentPeriodDate = firstNextPeriodPaycheck != null ? firstNextPeriodPaycheck.Date.AddDays(-1) : firstDayOfNextMonth.AddDays(-1);
        var endDateTransaction = transactions
            .Where(t => t.Date <= endOfCurrentPeriodDate && t.Date >= firstDayOfPeriod)
            .OrderByDescending(t => t.Date)
            .FirstOrDefault();

        string? endDateTransactionId = endDateTransaction?.TransactionId;
        return (startDateTransactionId, endDateTransactionId);
    }
    
    #endregion
    
}