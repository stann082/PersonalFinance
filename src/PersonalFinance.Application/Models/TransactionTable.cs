using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PersonalFinance.Domain.Misc;
using PersonalFinance.Domain.Models;

namespace PersonalFinance.Application.Models;

[Table(Constants.TransactionsTableName)]
public class TransactionTable
{
    [Key]
    [Column(Constants.TransactionsTableIdColumn)]
    public int Id { get; internal set; }

    [Column(Constants.TransactionsTableAmountColumn)]
    public decimal Amount { get; internal set; }

    [Column(Constants.TransactionsTableBankColumn)]
    public string Bank { get; internal set; }

    [Column(Constants.TransactionsTableCategoryColumn)]
    public string Category { get; internal set; }

    [Column(Constants.TransactionsTableDateColumn)]
    public DateTime Date { get; internal set; }

    [Column(Constants.TransactionsTableDescriptionColumn)]
    public string Description { get; internal set; }

    [Column(Constants.TransactionsTableTypeColumn)]
    public int Type { get; internal set; }

    public void CopyFrom(Transaction transaction)
    {
        Amount = transaction.Amount;
        Bank = transaction.Bank;
        Category = transaction.Category;
        Date = transaction.Date;
        Date = transaction.Date.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(transaction.Date, DateTimeKind.Utc)
            : transaction.Date.ToUniversalTime();
        Description = transaction.Description;
        Type = (int)transaction.TransactionType;
    }

}
