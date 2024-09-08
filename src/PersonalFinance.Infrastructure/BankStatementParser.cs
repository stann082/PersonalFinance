using System.Globalization;
using CsvHelper;
using PersonalFinance.Domain.Models;
using PersonalFinance.Infrastructure.Models;

namespace PersonalFinance.Infrastructure;

public class BankStatementParser : IBankStatementParser
{

    #region Public Methods

    public async Task<ITransactionMap[]> Parse(string filePath)
    {
        StreamReader? reader = null;
        CsvReader? csv = null;
        try
        {
            using (reader = new StreamReader(filePath))
            {
                using (csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    return await Task.FromResult(csv.GetRecords<BankOfAmericaTransactionMap>().ToArray());
                }
            }
        }
        catch (Exception e)
        {
            // log error
            return await Task.FromResult(Array.Empty<BankOfAmericaTransactionMap>());
        }
        finally
        {
            reader?.Dispose();
            csv?.Dispose();
        }
    }

    #endregion

}
