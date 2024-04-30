namespace PersonalFinance.Domain.Misc;

public static class Constants
{
    public const string BofA = "BofA";
    public const string CapitalOne = "Capital One";
    public const string Citi = "Citi";
    public const string TransactionsTableName = "transactions";
    public const string TransactionsTableAmountColumn = "amount";
    public const string TransactionsTableBankColumn = "bank";
    public const string TransactionsTableDateColumn = "date";
    public const string TransactionsTableDescriptionColumn = "description";
    public const string TransactionsTableCategoryColumn = "category";
    public const string TransactionsTableIdColumn = "id";
    public const string TransactionsTableTypeColumn = "type";
}

public enum TransactionType
{
    Debit = 0,
    Credit = 1
}
