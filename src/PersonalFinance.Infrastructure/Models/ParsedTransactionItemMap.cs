using System.Globalization;
using CsvHelper.Configuration;

namespace PersonalFinance.Infrastructure.Models;

public sealed class ParsedTransactionItemMap : ClassMap<ParsedTransactionItem>
{
    public ParsedTransactionItemMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(m => m.Category).Ignore();
        Map(m => m.IsRecurring).Ignore();
    }
}
