using System.Globalization;
using CsvHelper;
using PersonalFinance.Domain.Models;

namespace PersonalFinance.Infrastructure;

public static class BankOfAmericaStatementParser
{

    #region Public Methods

    public static async Task<ITransactionMapper[]> Parse(string filePath)
    {
        StreamReader? reader = null;
        CsvReader? csv = null;
        try
        {
            using (reader = new StreamReader(filePath))
            {
                using (csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    return await Task.FromResult(csv.GetRecords<BankOfAmericaTransactionMapper>().ToArray());
                }
            }
        }
        catch (Exception e)
        {
            // log error
            return await Task.FromResult(Array.Empty<ITransactionMapper>());
        }
        finally
        {
            reader?.Dispose();
            csv?.Dispose();
        }
    }

    #endregion

}
