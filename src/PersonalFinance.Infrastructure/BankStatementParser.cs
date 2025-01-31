using System.Globalization;
using CsvHelper;
using PersonalFinance.Domain.Models;
using PersonalFinance.Infrastructure.Models;
using Serilog;

namespace PersonalFinance.Infrastructure;

public class BankStatementParser : IBankStatementParser
{

    #region Public Methods

    public async Task<IParsedTransactionItem[]> Parse(string filePath)
    {
        StreamReader? reader = null;
        CsvReader? csv = null;
        try
        {
            using (reader = new StreamReader(filePath))
            {
                using (csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Context.RegisterClassMap<ParsedTransactionItemMap>();
                    var records = csv.GetRecords<ParsedTransactionItem>().ToArray();
                    return await Task.FromResult(records);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Something went wrong");
            return await Task.FromResult(Array.Empty<IParsedTransactionItem>());
        }
        finally
        {
            reader?.Dispose();
            csv?.Dispose();
            await Log.CloseAndFlushAsync();
        }
    }

    #endregion

}
