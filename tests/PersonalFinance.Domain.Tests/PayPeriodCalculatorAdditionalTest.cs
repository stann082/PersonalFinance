using PersonalFinance.Domain.Misc;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Util;

namespace PersonalFinance.Domain.Tests;

public class PayPeriodCalculatorAdditionalTest
{
    #region Edge Cases - Invalid Input

    [Test]
    public void Calculate_WithInvalidDateFormat_ThrowsArgumentException()
    {
        var transactions = Array.Empty<Transaction>();
        Assert.Throws<ArgumentException>(() => PayPeriodCalculator.Calculate(transactions, "invalid", "MYORG"));
    }

    [Test]
    public void Calculate_WithWrongDateFormat_ThrowsArgumentException()
    {
        var transactions = Array.Empty<Transaction>();
        Assert.Throws<ArgumentException>(() => PayPeriodCalculator.Calculate(transactions, "09/2024", "MYORG"));
    }

    [Test]
    public void Calculate_WithPartialDateFormat_ThrowsArgumentException()
    {
        var transactions = Array.Empty<Transaction>();
        Assert.Throws<ArgumentException>(() => PayPeriodCalculator.Calculate(transactions, "2024", "MYORG"));
    }

    #endregion

    #region Edge Cases - Empty Transactions

    [Test]
    public void Calculate_WithNoTransactions_ReturnsNulls()
    {
        var transactions = Array.Empty<Transaction>();
        var (startDateId, endDateId) = PayPeriodCalculator.Calculate(transactions, "September2024", "MYORG");
        Assert.Multiple(() =>
        {
            Assert.That(startDateId, Is.Null);
            Assert.That(endDateId, Is.Null);
        });
    }

    [Test]
    public void Calculate_WithNoMatchingTransactions_ReturnsNulls()
    {
        var transactions = new Transaction[]
        {
            new() { TransactionId = "1", Description = "OTHER", Date = new DateTime(2024, 9, 15) }
        };
        var (startDateId, endDateId) = PayPeriodCalculator.Calculate(transactions, "September2024", "MYORG");
        Assert.Multiple(() =>
        {
            Assert.That(startDateId, Is.Null);
            Assert.That(endDateId, Is.Null);
        });
    }

    #endregion

    #region Edge Cases - Boundary Dates

    [Test]
    public void Calculate_WithTransactionOnFirstDayOfMonth_ReturnsCorrectDates()
    {
        var transactions = new Transaction[]
        {
            new() { TransactionId = "1", Description = "MYORG", Date = new DateTime(2024, 8, 31) },
            new() { TransactionId = "2", Description = "Other", Date = new DateTime(2024, 9, 1) },
            new() { TransactionId = "3", Description = "MYORG", Date = new DateTime(2024, 9, 30) }
        };
        var (startDateId, endDateId) = PayPeriodCalculator.Calculate(transactions, "September2024", "MYORG");
        Assert.Multiple(() =>
        {
            Assert.That(startDateId, Is.EqualTo("1"));
            Assert.That(endDateId, Is.EqualTo("3"));
        });
    }

    [Test]
    public void Calculate_WithTransactionOnLastDayOfMonth_ReturnsCorrectDates()
    {
        var transactions = new Transaction[]
        {
            new() { TransactionId = "1", Description = "MYORG", Date = new DateTime(2024, 8, 15) },
            new() { TransactionId = "2", Description = "Other", Date = new DateTime(2024, 9, 1) },
            new() { TransactionId = "3", Description = "MYORG", Date = new DateTime(2024, 9, 30) }
        };
        var (startDateId, endDateId) = PayPeriodCalculator.Calculate(transactions, "September2024", "MYORG");
        Assert.That(endDateId, Is.EqualTo("3"));
    }

    #endregion

    #region Partial Month Scenarios

    [Test]
    public void Calculate_WithOnlyStartTransaction_ReturnsStartOnly()
    {
        var transactions = new Transaction[]
        {
            new() { TransactionId = "1", Description = "MYORG", Date = new DateTime(2024, 8, 30) }
        };
        var (startDateId, endDateId) = PayPeriodCalculator.Calculate(transactions, "September2024", "MYORG");
        Assert.Multiple(() =>
        {
            Assert.That(startDateId, Is.EqualTo("1"));
            Assert.That(endDateId, Is.Null);
        });
    }

    [Test]
    public void Calculate_WithOnlyEndTransaction_ReturnsEndOnly()
    {
        var transactions = new Transaction[]
        {
            new() { TransactionId = "1", Description = "MYORG", Date = new DateTime(2024, 9, 15) }
        };
        var (startDateId, endDateId) = PayPeriodCalculator.Calculate(transactions, "September2024", "MYORG");
        Assert.Multiple(() =>
        {
            Assert.That(startDateId, Is.Null);
            Assert.That(endDateId, Is.EqualTo("1"));
        });
    }

    [Test]
    public void Calculate_WithTransactionBeforeMonthOnly_ReturnsStartOnly()
    {
        var transactions = new Transaction[]
        {
            new() { TransactionId = "1", Description = "MYORG", Date = new DateTime(2024, 8, 20) },
            new() { TransactionId = "2", Description = "MYORG", Date = new DateTime(2024, 8, 28) }
        };
        var (startDateId, endDateId) = PayPeriodCalculator.Calculate(transactions, "September2024", "MYORG");
        Assert.Multiple(() =>
        {
            Assert.That(startDateId, Is.EqualTo("2"));
            Assert.That(endDateId, Is.Null);
        });
    }

    #endregion

    #region Multiple Transactions - Order Tests

    [Test]
    public void Calculate_WithMultipleStartTransactions_UsesLatest()
    {
        var transactions = new Transaction[]
        {
            new() { TransactionId = "1", Description = "MYORG", Date = new DateTime(2024, 8, 10) },
            new() { TransactionId = "2", Description = "MYORG", Date = new DateTime(2024, 8, 20) },
            new() { TransactionId = "3", Description = "MYORG", Date = new DateTime(2024, 8, 28) },
            new() { TransactionId = "4", Description = "Other", Date = new DateTime(2024, 9, 15) }
        };
        var (startDateId, endDateId) = PayPeriodCalculator.Calculate(transactions, "September2024", "MYORG");
        Assert.That(startDateId, Is.EqualTo("3"));
    }

    [Test]
    public void Calculate_WithMultipleEndTransactions_UsesLatest()
    {
        var transactions = new Transaction[]
        {
            new() { TransactionId = "1", Description = "MYORG", Date = new DateTime(2024, 8, 30) },
            new() { TransactionId = "2", Description = "MYORG", Date = new DateTime(2024, 9, 5) },
            new() { TransactionId = "3", Description = "MYORG", Date = new DateTime(2024, 9, 10) },
            new() { TransactionId = "4", Description = "MYORG", Date = new DateTime(2024, 9, 28) },
            new() { TransactionId = "5", Description = "MYORG", Date = new DateTime(2024, 10, 5) }
        };
        var (startDateId, endDateId) = PayPeriodCalculator.Calculate(transactions, "September2024", "MYORG");
        Assert.That(endDateId, Is.EqualTo("4"));
    }

    #endregion

    #region Description Matching Tests

    [Test]
    public void Calculate_WithPartialDescriptionMatch_MatchesCorrectly()
    {
        var transactions = new Transaction[]
        {
            new() { TransactionId = "1", Description = "SALARY FROM MYORG INC", Date = new DateTime(2024, 8, 30) },
            new() { TransactionId = "2", Description = "MYORG PAYMENT", Date = new DateTime(2024, 9, 28) }
        };
        var (startDateId, endDateId) = PayPeriodCalculator.Calculate(transactions, "September2024", "MYORG");
        Assert.Multiple(() =>
        {
            Assert.That(startDateId, Is.EqualTo("1"));
            Assert.That(endDateId, Is.EqualTo("2"));
        });
    }

    [Test]
    public void Calculate_WithCaseSensitiveDescription_DoesNotMatch()
    {
        var transactions = new Transaction[]
        {
            new() { TransactionId = "1", Description = "myorg", Date = new DateTime(2024, 8, 30) }
        };
        var (startDateId, endDateId) = PayPeriodCalculator.Calculate(transactions, "September2024", "MYORG");
        Assert.Multiple(() =>
        {
            Assert.That(startDateId, Is.Null);
            Assert.That(endDateId, Is.Null);
        });
    }

    [Test]
    public void Calculate_WithNullDescription_SkipsTransaction()
    {
        var transactions = new Transaction[]
        {
            new() { TransactionId = "1", Description = null, Date = new DateTime(2024, 8, 30) },
            new() { TransactionId = "2", Description = "MYORG", Date = new DateTime(2024, 8, 28) }
        };
        var (startDateId, endDateId) = PayPeriodCalculator.Calculate(transactions, "September2024", "MYORG");
        Assert.That(startDateId, Is.EqualTo("2"));
    }

    #endregion

    #region Different Months

    [Test]
    public void Calculate_WithJanuary_ReturnsCorrectPeriod()
    {
        var transactions = new Transaction[]
        {
            new() { TransactionId = "1", Description = "MYORG", Date = new DateTime(2024, 12, 30) },
            new() { TransactionId = "2", Description = "MYORG", Date = new DateTime(2024, 1, 25) }
        };
        var (startDateId, endDateId) = PayPeriodCalculator.Calculate(transactions, "January2024", "MYORG");
        Assert.Multiple(() =>
        {
            Assert.That(startDateId, Is.EqualTo("1"));
            Assert.That(endDateId, Is.EqualTo("2"));
        });
    }

    [Test]
    public void Calculate_WithDecember_ReturnsCorrectPeriod()
    {
        var transactions = new Transaction[]
        {
            new() { TransactionId = "1", Description = "MYORG", Date = new DateTime(2024, 11, 30) },
            new() { TransactionId = "2", Description = "MYORG", Date = new DateTime(2024, 12, 25) }
        };
        var (startDateId, endDateId) = PayPeriodCalculator.Calculate(transactions, "December2024", "MYORG");
        Assert.Multiple(() =>
        {
            Assert.That(startDateId, Is.EqualTo("1"));
            Assert.That(endDateId, Is.EqualTo("2"));
        });
    }

    #endregion

}
