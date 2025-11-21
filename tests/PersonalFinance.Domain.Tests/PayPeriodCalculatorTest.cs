using PersonalFinance.Domain.Misc;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Util;

namespace PersonalFinance.Domain.Tests;

public class PayPeriodCalculatorTest
{
    #region Blue Sky Tests

    [Test]
    public void BlueSkyTest_Parse()
    {
        // setup
        var transactions = new List<Transaction>();
        transactions.Add(new Transaction { TransactionId = "1XX01", Description = "DoorDash", Date = new DateTime(2024, 8, 27) });
        transactions.Add(new Transaction { TransactionId = "1XX02", Description = "MYORG", Date = new DateTime(2024, 8, 30) });
        transactions.Add(new Transaction { TransactionId = "1XX03", Description = "Mortgage", Date = new DateTime(2024, 9, 1) });
        transactions.Add(new Transaction { TransactionId = "1XX04", Description = "PetSmart", Date = new DateTime(2024, 9, 5) });
        transactions.Add(new Transaction { TransactionId = "1XX05", Description = "H-E-B", Date = new DateTime(2024, 9, 28) });
        transactions.Add(new Transaction { TransactionId = "1XX06", Description = "MYORG", Date = new DateTime(2024, 9, 28) });
        transactions.Add(new Transaction { TransactionId = "1XX07", Description = "Bling!", Date = new DateTime(2024, 9, 28) });

        // exercise
        var calculator = new PayPeriodCalculator();
        var (startDateId, endDateId) = calculator.Calculate(transactions.ToArray(), "September2024", "MYORG");

        // post-conditions
        Assert.Multiple(() =>
        {
            Assert.That(startDateId, Is.EqualTo("1XX02"));
            Assert.That(endDateId, Is.EqualTo("1XX05"));
        });
    }

    [Test]
    public void BlueSkyTest_Parse_IncompleteMonth()
    {
        // setup
        var transactions = new List<Transaction>();
        transactions.Add(new Transaction { TransactionId = "1XX01", Description = "DoorDash", Date = new DateTime(2024, 8, 27) });
        transactions.Add(new Transaction { TransactionId = "1XX02", Description = "MYORG", Date = new DateTime(2024, 8, 30) });
        transactions.Add(new Transaction { TransactionId = "1XX03", Description = "Mortgage", Date = new DateTime(2024, 9, 1) });
        transactions.Add(new Transaction { TransactionId = "1XX04", Description = "PetSmart", Date = new DateTime(2024, 9, 5) });

        // exercise
        var calculator = new PayPeriodCalculator();
        var (startDateId, endDateId) = calculator.Calculate(transactions.ToArray(), "September2024", "MYORG");

        // post-conditions
        Assert.Multiple(() =>
        {
            Assert.That(startDateId, Is.EqualTo("1XX02"));
            Assert.That(endDateId, Is.EqualTo("1XX04"));
        });
    }

    #endregion

}