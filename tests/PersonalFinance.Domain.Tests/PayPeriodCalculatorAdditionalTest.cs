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
        // Arrange
        var transactions = new ITransaction[] { };
        var calculator = new PayPeriodCalculator();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => calculator.Calculate(transactions, "invalid", "MYORG"));
    }

    [Test]
    public void Calculate_WithWrongDateFormat_ThrowsArgumentException()
    {
        // Arrange
        var transactions = new ITransaction[] { };
        var calculator = new PayPeriodCalculator();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => calculator.Calculate(transactions, "09/2024", "MYORG"));
    }

    [Test]
    public void Calculate_WithPartialDateFormat_ThrowsArgumentException()
    {
        // Arrange
        var transactions = new ITransaction[] { };
        var calculator = new PayPeriodCalculator();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => calculator.Calculate(transactions, "2024", "MYORG"));
    }

    #endregion

    #region Edge Cases - Empty Transactions

    [Test]
    public void Calculate_WithNoTransactions_ReturnsNulls()
    {
        // Arrange
        var transactions = Array.Empty<ITransaction>();
        var calculator = new PayPeriodCalculator();

        // Act
        var (startDateId, endDateId) = calculator.Calculate(transactions, "September2024", "MYORG");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(startDateId, Is.Null);
            Assert.That(endDateId, Is.Null);
        });
    }

    [Test]
    public void Calculate_WithNoMatchingTransactions_ReturnsNulls()
    {
        // Arrange
        var transactions = new ITransaction[]
        {
            new MockTransaction { TransactionId = "1", Description = "OTHER", Date = new DateTime(2024, 9, 15) }
        };
        var calculator = new PayPeriodCalculator();

        // Act
        var (startDateId, endDateId) = calculator.Calculate(transactions, "September2024", "MYORG");

        // Assert
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
        // Arrange
        var transactions = new ITransaction[]
        {
            new MockTransaction { TransactionId = "1", Description = "MYORG", Date = new DateTime(2024, 8, 31) },
            new MockTransaction { TransactionId = "2", Description = "Other", Date = new DateTime(2024, 9, 1) },
            new MockTransaction { TransactionId = "3", Description = "MYORG", Date = new DateTime(2024, 9, 30) }
        };
        var calculator = new PayPeriodCalculator();

        // Act
        var (startDateId, endDateId) = calculator.Calculate(transactions, "September2024", "MYORG");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(startDateId, Is.EqualTo("1"));
            Assert.That(endDateId, Is.EqualTo("3"));
        });
    }

    [Test]
    public void Calculate_WithTransactionOnLastDayOfMonth_ReturnsCorrectDates()
    {
        // Arrange
        var transactions = new ITransaction[]
        {
            new MockTransaction { TransactionId = "1", Description = "MYORG", Date = new DateTime(2024, 8, 15) },
            new MockTransaction { TransactionId = "2", Description = "Other", Date = new DateTime(2024, 9, 1) },
            new MockTransaction { TransactionId = "3", Description = "MYORG", Date = new DateTime(2024, 9, 30) }
        };
        var calculator = new PayPeriodCalculator();

        // Act
        var (startDateId, endDateId) = calculator.Calculate(transactions, "September2024", "MYORG");

        // Assert
        Assert.That(endDateId, Is.EqualTo("3"));
    }

    #endregion

    #region Partial Month Scenarios

    [Test]
    public void Calculate_WithOnlyStartTransaction_ReturnsStartOnly()
    {
        // Arrange
        var transactions = new ITransaction[]
        {
            new MockTransaction { TransactionId = "1", Description = "MYORG", Date = new DateTime(2024, 8, 30) }
        };
        var calculator = new PayPeriodCalculator();

        // Act
        var (startDateId, endDateId) = calculator.Calculate(transactions, "September2024", "MYORG");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(startDateId, Is.EqualTo("1"));
            Assert.That(endDateId, Is.Null);
        });
    }

    [Test]
    public void Calculate_WithOnlyEndTransaction_ReturnsEndOnly()
    {
        // Arrange
        var transactions = new ITransaction[]
        {
            new MockTransaction { TransactionId = "1", Description = "MYORG", Date = new DateTime(2024, 9, 15) }
        };
        var calculator = new PayPeriodCalculator();

        // Act
        var (startDateId, endDateId) = calculator.Calculate(transactions, "September2024", "MYORG");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(startDateId, Is.Null);
            Assert.That(endDateId, Is.EqualTo("1"));
        });
    }

    [Test]
    public void Calculate_WithTransactionBeforeMonthOnly_ReturnsStartOnly()
    {
        // Arrange
        var transactions = new ITransaction[]
        {
            new MockTransaction { TransactionId = "1", Description = "MYORG", Date = new DateTime(2024, 8, 20) },
            new MockTransaction { TransactionId = "2", Description = "MYORG", Date = new DateTime(2024, 8, 28) }
        };
        var calculator = new PayPeriodCalculator();

        // Act
        var (startDateId, endDateId) = calculator.Calculate(transactions, "September2024", "MYORG");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(startDateId, Is.EqualTo("2")); // Most recent
            Assert.That(endDateId, Is.Null);
        });
    }

    #endregion

    #region Multiple Transactions - Order Tests

    [Test]
    public void Calculate_WithMultipleStartTransactions_UsesLatest()
    {
        // Arrange
        var transactions = new ITransaction[]
        {
            new MockTransaction { TransactionId = "1", Description = "MYORG", Date = new DateTime(2024, 8, 10) },
            new MockTransaction { TransactionId = "2", Description = "MYORG", Date = new DateTime(2024, 8, 20) },
            new MockTransaction { TransactionId = "3", Description = "MYORG", Date = new DateTime(2024, 8, 28) },
            new MockTransaction { TransactionId = "4", Description = "Other", Date = new DateTime(2024, 9, 15) }
        };
        var calculator = new PayPeriodCalculator();

        // Act
        var (startDateId, endDateId) = calculator.Calculate(transactions, "September2024", "MYORG");

        // Assert
        Assert.That(startDateId, Is.EqualTo("3")); // Latest before month
    }

    [Test]
    public void Calculate_WithMultipleEndTransactions_UsesLatest()
    {
        // Arrange
        var transactions = new ITransaction[]
        {
            new MockTransaction { TransactionId = "1", Description = "MYORG", Date = new DateTime(2024, 8, 30) },
            new MockTransaction { TransactionId = "2", Description = "MYORG", Date = new DateTime(2024, 9, 5) },
            new MockTransaction { TransactionId = "3", Description = "MYORG", Date = new DateTime(2024, 9, 10) },
            new MockTransaction { TransactionId = "4", Description = "MYORG", Date = new DateTime(2024, 9, 28) },
            new MockTransaction { TransactionId = "5", Description = "MYORG", Date = new DateTime(2024, 10, 5) }
        };
        var calculator = new PayPeriodCalculator();

        // Act
        var (startDateId, endDateId) = calculator.Calculate(transactions, "September2024", "MYORG");

        // Assert
        Assert.That(endDateId, Is.EqualTo("4")); // Latest in month but before next paycheck
    }

    #endregion

    #region Description Matching Tests

    [Test]
    public void Calculate_WithPartialDescriptionMatch_MatchesCorrectly()
    {
        // Arrange
        var transactions = new ITransaction[]
        {
            new MockTransaction { TransactionId = "1", Description = "SALARY FROM MYORG INC", Date = new DateTime(2024, 8, 30) },
            new MockTransaction { TransactionId = "2", Description = "MYORG PAYMENT", Date = new DateTime(2024, 9, 28) }
        };
        var calculator = new PayPeriodCalculator();

        // Act
        var (startDateId, endDateId) = calculator.Calculate(transactions, "September2024", "MYORG");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(startDateId, Is.EqualTo("1"));
            Assert.That(endDateId, Is.EqualTo("2"));
        });
    }

    [Test]
    public void Calculate_WithCaseSensitiveDescription_DoesNotMatch()
    {
        // Arrange
        var transactions = new ITransaction[]
        {
            new MockTransaction { TransactionId = "1", Description = "myorg", Date = new DateTime(2024, 8, 30) }
        };
        var calculator = new PayPeriodCalculator();

        // Act
        var (startDateId, endDateId) = calculator.Calculate(transactions, "September2024", "MYORG");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(startDateId, Is.Null);
            Assert.That(endDateId, Is.Null);
        });
    }

    [Test]
    public void Calculate_WithNullDescription_SkipsTransaction()
    {
        // Arrange
        var transactions = new ITransaction[]
        {
            new MockTransaction { TransactionId = "1", Description = null, Date = new DateTime(2024, 8, 30) },
            new MockTransaction { TransactionId = "2", Description = "MYORG", Date = new DateTime(2024, 8, 28) }
        };
        var calculator = new PayPeriodCalculator();

        // Act
        var (startDateId, endDateId) = calculator.Calculate(transactions, "September2024", "MYORG");

        // Assert
        Assert.That(startDateId, Is.EqualTo("2"));
    }

    #endregion

    #region Different Months

    [Test]
    public void Calculate_WithJanuary_ReturnsCorrectPeriod()
    {
        // Arrange
        var transactions = new ITransaction[]
        {
            new MockTransaction { TransactionId = "1", Description = "MYORG", Date = new DateTime(2024, 12, 30) },
            new MockTransaction { TransactionId = "2", Description = "MYORG", Date = new DateTime(2024, 1, 25) }
        };
        var calculator = new PayPeriodCalculator();

        // Act
        var (startDateId, endDateId) = calculator.Calculate(transactions, "January2024", "MYORG");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(startDateId, Is.EqualTo("1"));
            Assert.That(endDateId, Is.EqualTo("2"));
        });
    }

    [Test]
    public void Calculate_WithDecember_ReturnsCorrectPeriod()
    {
        // Arrange
        var transactions = new ITransaction[]
        {
            new MockTransaction { TransactionId = "1", Description = "MYORG", Date = new DateTime(2024, 11, 30) },
            new MockTransaction { TransactionId = "2", Description = "MYORG", Date = new DateTime(2024, 12, 25) }
        };
        var calculator = new PayPeriodCalculator();

        // Act
        var (startDateId, endDateId) = calculator.Calculate(transactions, "December2024", "MYORG");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(startDateId, Is.EqualTo("1"));
            Assert.That(endDateId, Is.EqualTo("2"));
        });
    }

    #endregion

    #region Helper Classes

    private class MockTransaction : ITransaction
    {
        public decimal Amount => 0;
        public decimal Balance => 0;
        public string? Category { get; set; }
        public DateTime Date { get; init; }
        public string? Description { get; set; }
        public bool IsRecurring { get; set; }
        public string? TransactionId { get; init; }
        public TransactionType TransactionType => TransactionType.Credit;
    }

    #endregion
}
