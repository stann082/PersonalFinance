using PersonalFinance.CLI.Util;

namespace PersonalFinance.Application.Tests;

public class PersonalNameObscurerTest
{
    [Test]
    public void Obscure_ReplacesSimpleFirstLast()
    {
        var actual = PersonalNameObscurer.Obscure("JON SMITH", "Jon", "SMITH");

        Assert.That(actual, Is.EqualTo("[REDACTED]"));
    }

    [Test]
    public void Obscure_ReplacesLastFirstWithoutComma()
    {
        var actual = PersonalNameObscurer.Obscure("SMITH JON", "Jon", "SMITH");

        Assert.That(actual, Is.EqualTo("[REDACTED]"));
    }

    [Test]
    public void Obscure_ReplacesLastFirstWithComma()
    {
        var actual = PersonalNameObscurer.Obscure("SMITH, JON", "Jon", "SMITH");

        Assert.That(actual, Is.EqualTo("[REDACTED]"));
    }

    [Test]
    public void Obscure_ReplacesNameWithMiddleInitial()
    {
        var actual = PersonalNameObscurer.Obscure("PAYROLL INDN:JON A. SMITH CO", "Jon", "SMITH");

        Assert.That(actual, Is.EqualTo("PAYROLL INDN:[REDACTED] CO"));
    }

    [Test]
    public void Obscure_ReplacesNameWithPunctuationSeparator()
    {
        var actual = PersonalNameObscurer.Obscure("INDN:JON*SMITH CO", "Jon", "SMITH");

        Assert.That(actual, Is.EqualTo("INDN:[REDACTED] CO"));
    }

    [Test]
    public void Obscure_IsCaseInsensitive()
    {
        var actual = PersonalNameObscurer.Obscure("JON SMITH", "Jon", "SMITH");

        Assert.That(actual, Is.EqualTo("[REDACTED]"));
    }

    [Test]
    public void Obscure_ReplacesMultipleOccurrences()
    {
        var actual = PersonalNameObscurer.Obscure("JON SMITH and SMITH Jon are paid", "Jon", "SMITH");

        Assert.That(actual, Is.EqualTo("[REDACTED] and [REDACTED] are paid"));
    }

    [Test]
    public void Obscure_DoesNotChangeUnaffectedDescription()
    {
        var input = "PAYROLL INDN:JOHN SMITH CO";

        var actual = PersonalNameObscurer.Obscure(input, "JON", "SMITH");

        Assert.That(actual, Is.EqualTo(input));
    }

    [Test]
    public void Obscure_HandlesNullDescriptionAsEmptyString()
    {
        var actual = PersonalNameObscurer.Obscure(null, "JON", "SMITH");

        Assert.That(actual, Is.EqualTo(string.Empty));
    }
}
