using PersonalFinance.Domain.Util;

namespace PersonalFinance.Domain.Tests;

public class SimpleFinCredentialsTest
{

    [Test]
    public void Parse_SplitsCredentialsOutOfAccessUrl()
    {
        // Arrange
        var credentials = new SimpleFinCredentials();

        // Act
        var result = credentials.Parse("https://user123:pass456@bridge.simplefin.org/simplefin");

        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(credentials.Username, Is.EqualTo("user123"));
        Assert.That(credentials.Password, Is.EqualTo("pass456"));
        Assert.That(credentials.BaseUrl, Is.EqualTo("https://bridge.simplefin.org/simplefin"));
    }

    [Test]
    public void Parse_TrimsTrailingSlashSoAccountsPathIsWellFormed()
    {
        // Arrange
        var credentials = new SimpleFinCredentials();

        // Act
        credentials.Parse("https://u:p@bridge.simplefin.org/simplefin/");

        // Assert
        Assert.That(credentials.BaseUrl, Is.EqualTo("https://bridge.simplefin.org/simplefin"));
    }

    [Test]
    public void Parse_UnescapesPercentEncodedCredentials()
    {
        // Arrange
        var credentials = new SimpleFinCredentials();

        // Act
        credentials.Parse("https://user%40example.com:p%3Aass@bridge.simplefin.org/simplefin");

        // Assert
        Assert.That(credentials.Username, Is.EqualTo("user@example.com"));
        Assert.That(credentials.Password, Is.EqualTo("p:ass"));
    }

    [Test]
    public void Parse_IgnoresSurroundingWhitespace()
    {
        // Arrange
        var credentials = new SimpleFinCredentials();

        // Act
        var result = credentials.Parse("  https://u:p@bridge.simplefin.org/simplefin  ");

        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(credentials.BaseUrl, Is.EqualTo("https://bridge.simplefin.org/simplefin"));
    }

    [Test]
    public void Parse_FailsWhenCredentialsAreMissing()
    {
        // Arrange
        var credentials = new SimpleFinCredentials();

        // Act
        var result = credentials.Parse("https://bridge.simplefin.org/simplefin");

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Error, Does.Contain("credentials"));
    }

    [Test]
    public void Parse_FailsOnGarbage()
    {
        // Arrange
        var credentials = new SimpleFinCredentials();

        // Act
        var result = credentials.Parse("not-a-url");

        // Assert
        Assert.That(result.IsValid, Is.False);
    }

}
