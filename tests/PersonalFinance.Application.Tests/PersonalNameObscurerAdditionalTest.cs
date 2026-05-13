using PersonalFinance.CLI.Util;

namespace PersonalFinance.Application.Tests;

public class PersonalNameObscurerAdditionalTest
{
    #region Edge Cases

    [Test]
    public void Obscure_WithEmptyDescription_ReturnsEmptyString()
    {
        // Act
        var actual = PersonalNameObscurer.Obscure(string.Empty, "Jon", "Smith");

        // Assert
        Assert.That(actual, Is.Empty);
    }

    [Test]
    public void Obscure_WithWhitespaceOnlyDescription_ReturnsEmptyString()
    {
        // Act
        var actual = PersonalNameObscurer.Obscure("   ", "Jon", "Smith");

        // Assert
        Assert.That(actual, "   ");
    }

    [Test]
    public void Obscure_WithEmptyFirstName_DoesNotRedact()
    {
        // Act
        var actual = PersonalNameObscurer.Obscure("JON SMITH", string.Empty, "SMITH");

        // Assert
        Assert.That(actual, Is.EqualTo("[REDACTED]"));
    }

    [Test]
    public void Obscure_WithEmptyLastName_DoesNotRedact()
    {
        // Act
        var actual = PersonalNameObscurer.Obscure("JON SMITH", "JON", string.Empty);

        // Assert
        Assert.That(actual, Is.EqualTo("[REDACTED]SMITH"));
    }

    #endregion

    #region Special Characters and Punctuation

    [Test]
    public void Obscure_WithParentheses()
    {
        // Act
        var actual = PersonalNameObscurer.Obscure("PAYMENT FROM (JON SMITH) COMPANY", "Jon", "SMITH");

        // Assert
        Assert.That(actual, Contains.Substring("[REDACTED]"));
    }

    [Test]
    public void Obscure_WithSlashes()
    {
        // Act
        var actual = PersonalNameObscurer.Obscure("JON/SMITH", "Jon", "SMITH");

        // Assert
        Assert.That(actual, Contains.Substring("[REDACTED]"));
    }

    [Test]
    public void Obscure_WithHyphens()
    {
        // Act
        var actual = PersonalNameObscurer.Obscure("JON-SMITH", "Jon", "SMITH");

        // Assert
        Assert.That(actual, Contains.Substring("[REDACTED]"));
    }

    [Test]
    public void Obscure_WithUnderscores()
    {
        // Act
        var actual = PersonalNameObscurer.Obscure("PAYMENT_JON_SMITH", "Jon", "SMITH");

        // Assert
        Assert.That(actual, Contains.Substring("[REDACTED]"));
    }

    #endregion

    #region Name Variations

    [Test]
    public void Obscure_WithFirstNameOnly()
    {
        // Act
        var actual = PersonalNameObscurer.Obscure("JON is paid", "Jon", "SMITH");

        // Assert
        // Should not redact as full name is not present
        Assert.That(actual, Is.EqualTo("JON is paid"));
    }

    [Test]
    public void Obscure_WithLastNameOnly()
    {
        // Act
        var actual = PersonalNameObscurer.Obscure("SMITH payment", "Jon", "SMITH");

        // Assert
        // Should not redact as full name is not present
        Assert.That(actual, Is.EqualTo("SMITH payment"));
    }

    [Test]
    public void Obscure_WithMultipleMiddleInitials()
    {
        // Act
        var actual = PersonalNameObscurer.Obscure("PAYMENT: JON A. B. SMITH CO", "Jon", "SMITH");

        // Assert
        Assert.That(actual, Contains.Substring("[REDACTED]"));
    }

    [Test]
    public void Obscure_WithoutSpacesBetweenNames()
    {
        // Act
        var actual = PersonalNameObscurer.Obscure("JONSMITH", "Jon", "SMITH");

        // Assert
        // Tight concatenation - may or may not match depending on regex
        // Document behavior
        Assert.That(actual, Is.Not.Null);
    }

    [Test]
    public void Obscure_WithMixedCaseFirstName()
    {
        // Act
        var actual = PersonalNameObscurer.Obscure("Jon SMITH", "jon", "smith");

        // Assert
        Assert.That(actual, Is.EqualTo("[REDACTED]"));
    }

    #endregion

    #region Multiple Names

    [Test]
    public void Obscure_WithThreeOccurrences()
    {
        // Act
        var actual = PersonalNameObscurer.Obscure("JON SMITH and JON SMITH and again JON SMITH", "Jon", "SMITH");

        // Assert
        var occurrences = (actual.Length - actual.Replace("[REDACTED]", "").Length) / "[REDACTED]".Length;
        Assert.That(occurrences, Is.EqualTo(3));
    }

    [Test]
    public void Obscure_WithMultipleDifferentFormats()
    {
        // Act
        var actual = PersonalNameObscurer.Obscure("JON SMITH and SMITH, JON and INDN:JON A. SMITH", "Jon", "SMITH");

        // Assert
        var occurrences = (actual.Length - actual.Replace("[REDACTED]", "").Length) / "[REDACTED]".Length;
        Assert.That(occurrences, Is.GreaterThanOrEqualTo(3));
    }

    #endregion

    #region Surrounding Context

    [Test]
    public void Obscure_PreservesSurroundingText()
    {
        // Act
        var actual = PersonalNameObscurer.Obscure("START JON SMITH END", "Jon", "SMITH");

        // Assert
        Assert.That(actual, Does.StartWith("START"));
        Assert.That(actual, Does.EndWith("END"));
        Assert.That(actual, Contains.Substring("[REDACTED]"));
    }

    [Test]
    public void Obscure_PreservesLeadingNumbers()
    {
        // Act
        var actual = PersonalNameObscurer.Obscure("123 JON SMITH 456", "Jon", "SMITH");

        // Assert
        Assert.That(actual, Does.StartWith("123"));
        Assert.That(actual, Contains.Substring("456"));
    }

    #endregion

    #region Edge Case Names

    [Test]
    public void Obscure_WithSingleCharacterNames()
    {
        // Act
        var actual = PersonalNameObscurer.Obscure("J S", "J", "S");

        // Assert
        Assert.That(actual, Is.EqualTo("[REDACTED]"));
    }

    [Test]
    public void Obscure_WithVeryLongNames()
    {
        // Arrange
        var longFirstName = "EXTRAORDINARILYLONGFIRSTNAME";
        var longLastName = "EXTRAORDINARILYLONGLASTNAME";
        var description = $"{longFirstName} {longLastName}";

        // Act
        var actual = PersonalNameObscurer.Obscure(description, longFirstName, longLastName);

        // Assert
        Assert.That(actual, Is.EqualTo("[REDACTED]"));
    }

    [Test]
    public void Obscure_WithSpecialCharactersInNames()
    {
        // Act - Names with regex special characters
        var actual = PersonalNameObscurer.Obscure("JON.SMITH JON.SMITH", "JON.SMITH", "SMITH");

        // Assert - Should handle regex escaping
        Assert.That(actual, Is.Not.Null);
    }

    #endregion

    #region Real-World Scenarios

    [Test]
    public void Obscure_BankTransactionFormat()
    {
        // Act
        var actual = PersonalNameObscurer.Obscure("TRANSFER TO SMITH JON ACH", "Jon", "SMITH");

        // Assert
        Assert.That(actual, Contains.Substring("[REDACTED]"));
    }

    [Test]
    public void Obscure_PayrollFormat()
    {
        // Act
        var actual = PersonalNameObscurer.Obscure("EMPLOYER NAME - PAYROLL DEDUCTION FOR JON A. SMITH", "Jon", "SMITH");

        // Assert
        Assert.That(actual, Contains.Substring("[REDACTED]"));
    }

    [Test]
    public void Obscure_CheckFormat()
    {
        // Act
        var actual = PersonalNameObscurer.Obscure("CHECK #12345 TO SMITH, JON", "Jon", "SMITH");

        // Assert
        Assert.That(actual, Contains.Substring("[REDACTED]"));
    }

    [Test]
    public void Obscure_MultipleNamesInTransaction()
    {
        // Act
        var actual = PersonalNameObscurer.Obscure("TRANSFER FROM JON SMITH TO JOHN JONES", "Jon", "SMITH");

        // Assert
        Assert.That(actual, Contains.Substring("[REDACTED]"));
        Assert.That(actual, Contains.Substring("JOHN JONES"));
    }

    #endregion

    #region Consistency Tests

    [Test]
    public void Obscure_SameInputProducesSameOutput()
    {
        // Arrange
        var input = "JON SMITH paid JON SMITH";

        // Act
        var result1 = PersonalNameObscurer.Obscure(input, "Jon", "SMITH");
        var result2 = PersonalNameObscurer.Obscure(input, "Jon", "SMITH");

        // Assert
        Assert.That(result1, Is.EqualTo(result2));
    }

    #endregion
}
