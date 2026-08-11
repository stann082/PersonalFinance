namespace PersonalFinance.Domain.Util;

/// <summary>
/// Reads the SimpleFIN access URL from the environment and splits its embedded
/// HTTP Basic credentials out of the userinfo component.
/// </summary>
public class SimpleFinCredentials
{

    #region Constants

    public const string EnvironmentVariableName = "SIMPLEFIN_ACCESS_URL";

    #endregion

    #region Properties

    public string BaseUrl { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;

    #endregion

    #region Public Methods

    public (string Error, bool IsValid) Validate()
    {
        string? accessUrl = Environment.GetEnvironmentVariable(EnvironmentVariableName);
        if (string.IsNullOrWhiteSpace(accessUrl))
        {
            return ($"{EnvironmentVariableName} environment variable is not found. Run 'fin simplefin-claim <setup-token>' first.", false);
        }

        return Parse(accessUrl);
    }

    /// <summary>
    /// Splits an access URL of the form https://user:pass@host/path into its parts.
    /// </summary>
    public (string Error, bool IsValid) Parse(string accessUrl)
    {
        if (!Uri.TryCreate(accessUrl.Trim(), UriKind.Absolute, out var uri))
        {
            return ($"{EnvironmentVariableName} is not a valid absolute URL.", false);
        }

        string[] userInfo = uri.UserInfo.Split(':', 2);
        if (userInfo.Length != 2 || string.IsNullOrEmpty(userInfo[0]))
        {
            return ($"{EnvironmentVariableName} does not contain the expected user:password credentials.", false);
        }

        Username = Uri.UnescapeDataString(userInfo[0]);
        Password = Uri.UnescapeDataString(userInfo[1]);

        var builder = new UriBuilder(uri) { UserName = string.Empty, Password = string.Empty };
        BaseUrl = builder.Uri.GetLeftPart(UriPartial.Path).TrimEnd('/');

        return (string.Empty, true);
    }

    #endregion

}
