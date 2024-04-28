namespace PersonalFinance.Domain.Misc;

public static class Constants
{
    public const string BofA = "BofA";
    public const string CapitalOne = "Capital One";
    public const string Citi = "Citi";
}

public enum TransactionType
{
    Debit = 0,
    Credit = 1
}
