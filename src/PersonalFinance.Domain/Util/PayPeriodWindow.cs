using System.Globalization;

namespace PersonalFinance.Domain.Util;

public static class PayPeriodWindow
{

    #region Public Methods

    public static (DateTime windowStart, DateTime windowEnd) GetWindow(string payPeriod)
    {
        if (!DateTime.TryParseExact(payPeriod, "MMMMyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var periodDate))
        {
            throw new ArgumentException("Invalid date format. Use 'MonthYear', e.g., 'September2024'.");
        }

        var firstDayOfPeriod = new DateTime(periodDate.Year, periodDate.Month, 1);
        var windowStart = firstDayOfPeriod.AddMonths(-1);
        var windowEnd = firstDayOfPeriod.AddMonths(2).AddDays(-1);
        return (windowStart, windowEnd);
    }

    #endregion
    
}

