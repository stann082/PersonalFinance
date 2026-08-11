using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;
using PersonalFinance.Domain.Util;

namespace PersonalFinance.Infrastructure.SimpleFin;

public class SimpleFinClient(HttpClient httpClient) : ISimpleFinClient
{

    #region Public Methods

    public async Task<SimpleFinAccountSet> GetAccountsAsync(DateTimeOffset startDate, DateTimeOffset endDate,
        bool includePending, CancellationToken cancellationToken = default)
    {
        var credentials = new SimpleFinCredentials();
        var validation = credentials.Validate();
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(validation.Error);
        }

        var query = new List<string>
        {
            "version=2",
            $"start-date={startDate.ToUnixTimeSeconds()}",
            $"end-date={endDate.ToUnixTimeSeconds()}"
        };

        if (includePending)
        {
            query.Add("pending=1");
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"{credentials.BaseUrl}/accounts?{string.Join('&', query)}");
        request.Headers.Authorization = BuildBasicAuthHeader(credentials.Username, credentials.Password);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"SimpleFIN returned {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        }

        return JsonSerializer.Deserialize<SimpleFinAccountSet>(body) ?? new SimpleFinAccountSet();
    }

    public async Task<string> ClaimAsync(string setupToken, CancellationToken cancellationToken = default)
    {
        string claimUrl = DecodeSetupToken(setupToken);

        using var response = await httpClient.PostAsync(claimUrl, new StringContent(string.Empty), cancellationToken);
        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Claim failed with {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        }

        return body.Trim();
    }

    #endregion

    #region Helper Methods

    private static AuthenticationHeaderValue BuildBasicAuthHeader(string username, string password)
    {
        string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        return new AuthenticationHeaderValue("Basic", encoded);
    }

    /// <summary>
    /// A setup token is the base64 encoding of the one-time claim URL.
    /// </summary>
    private static string DecodeSetupToken(string setupToken)
    {
        try
        {
            string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(setupToken.Trim())).Trim();
            if (!Uri.TryCreate(decoded, UriKind.Absolute, out _))
            {
                throw new FormatException($"Setup token decoded to '{decoded}', which is not a URL.");
            }

            return decoded;
        }
        catch (FormatException ex)
        {
            throw new FormatException("Setup token is not a valid base64-encoded claim URL.", ex);
        }
    }

    #endregion

}
